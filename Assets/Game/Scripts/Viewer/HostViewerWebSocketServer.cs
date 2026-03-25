using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoulForge.Bootstrap;
using SoulForge.Economy;
using SoulForge.Enemies;
using SoulForge.Player;
using SoulForge.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Viewer
{
    public sealed class HostViewerWebSocketServer : MonoBehaviour
    {
        private static HostViewerWebSocketServer instance;
        private static readonly UTF8Encoding Utf8NoBom = new(false);

        [Serializable]
        private sealed class JoinRequest
        {
            public string ViewerId;
            public string SessionId;
        }

        [Serializable]
        private sealed class PurchaseRequest
        {
            public string CommandId;
            public string ViewerId;
            public string ActionId;
            public string TargetId;
            public bool HasViewportTarget;
            public float ViewportX;
            public float ViewportY;
        }

        private sealed class ClientConnection
        {
            public string ViewerId;
            public TcpClient Client;
            public StreamReader Reader;
            public StreamWriter Writer;
            public readonly object WriteLock = new();
        }

        [SerializeField] private int port = 8080;
        [SerializeField] private string listenPrefix = "tcp://0.0.0.0:8080";
        [SerializeField] private ViewerSessionService sessionService;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerActionExecutor actionExecutor;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private RunController runController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;
        [SerializeField] private int debugTopUpBalance = 9999;

        private readonly ConcurrentQueue<Action> mainThreadActions = new();
        private readonly List<ClientConnection> clients = new();
        private readonly object clientsLock = new();
        private readonly HashSet<string> rewardedViewerIds = new();

        private CancellationTokenSource cancellation;
        private TcpListener listener;

        public static HostViewerWebSocketServer Instance => instance;
        public string ListenPrefix => listenPrefix;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            RebindSceneReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SubscribeBroadcaster(stateBroadcaster);
            StartServer();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SubscribeBroadcaster(null);
            StopServer();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            while (mainThreadActions.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }

        [ContextMenu("Start Server")]
        public void StartServer()
        {
            if (listener != null)
            {
                return;
            }

            cancellation = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);

            try
            {
                listener.Start();
                listenPrefix = $"tcp://0.0.0.0:{port}";
                _ = AcceptLoopAsync(cancellation.Token);
                Debug.Log($"Host viewer TCP server listening on {listenPrefix}");
                if (sessionService != null)
                {
                    Debug.Log($"Host session code: {sessionService.SessionId}");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to start host TCP server on port {port}: {exception.Message}");
                listener.Stop();
                listener = null;
                cancellation.Dispose();
                cancellation = null;
            }
        }

        [ContextMenu("Stop Server")]
        public void StopServer()
        {
            if (listener == null)
            {
                return;
            }

            cancellation?.Cancel();

            lock (clientsLock)
            {
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    _ = CloseClientAsync(clients[i]);
                }

                clients.Clear();
            }

            listener.Stop();
            listener = null;
            cancellation?.Dispose();
            cancellation = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebindSceneReferences();
            EnsureBudgetInitialized();
            GrantJoinRewardsForConnectedViewers();
            BroadcastSnapshot(BuildSnapshot());
        }


        private void RebindSceneReferences()
        {
            if (sessionService == null)
            {
                sessionService = FindFirstObjectByType<ViewerSessionService>();
            }

            StateBroadcaster nextBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            if (nextBroadcaster != stateBroadcaster)
            {
                SubscribeBroadcaster(nextBroadcaster);
            }

            actionExecutor = FindFirstObjectByType<ViewerActionExecutor>();
            economyService = FindFirstObjectByType<ViewerEconomyService>();
            runController = FindFirstObjectByType<RunController>();
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            viewerActionQueue = FindFirstObjectByType<ViewerActionQueue>();
            roomBudgetService = FindFirstObjectByType<ViewerRoomBudgetService>();
        }

        private void SubscribeBroadcaster(StateBroadcaster nextBroadcaster)
        {
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast -= BroadcastSnapshot;
                stateBroadcaster.DeltaBroadcast -= BroadcastDelta;
                stateBroadcaster.ActionResultBroadcast -= BroadcastResult;
            }

            stateBroadcaster = nextBroadcaster;

            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast += BroadcastSnapshot;
                stateBroadcaster.DeltaBroadcast += BroadcastDelta;
                stateBroadcaster.ActionResultBroadcast += BroadcastResult;
            }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && listener != null)
            {
                TcpClient tcpClient;
                try
                {
                    tcpClient = await listener.AcceptTcpClientAsync();
                }
                catch
                {
                    break;
                }

                try
                {
                    NetworkStream stream = tcpClient.GetStream();
                    ClientConnection client = new()
                    {
                        Client = tcpClient,
                        Reader = new StreamReader(stream, Utf8NoBom),
                        Writer = new StreamWriter(stream, Utf8NoBom) { AutoFlush = true }
                    };
                    client.Client.NoDelay = true;

                    lock (clientsLock)
                    {
                        clients.Add(client);
                    }

                    Debug.Log($"Viewer TCP client accepted: {client.Client.Client.RemoteEndPoint}");
                    _ = ReceiveLoopAsync(client, token);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Failed to accept TCP client: {exception.Message}");
                    tcpClient.Dispose();
                }
            }
        }

        private async Task ReceiveLoopAsync(ClientConnection client, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && client.Reader != null)
                {
                    string message = await client.Reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Debug.Log($"Viewer TCP client closed stream: {client.ViewerId}");
                        break;
                    }

                    mainThreadActions.Enqueue(() => ProcessIncoming(client, message));
                }
            }
            catch (Exception exception)
            {
                if (!token.IsCancellationRequested && client != null && client.Client != null && client.Client.Connected)
                {
                    Debug.LogWarning($"Viewer TCP client disconnected: {exception.Message}");
                }
            }
            finally
            {
                await CloseClientAsync(client);
            }
        }

        private void ProcessIncoming(ClientConnection client, string message)
        {
            int separatorIndex = message.IndexOf('|');
            if (separatorIndex <= 0)
            {
                return;
            }

            string type = message[..separatorIndex];
            string payload = message[(separatorIndex + 1)..];
            type = type.TrimStart('\uFEFF');

            switch (type)
            {
                case "join":
                {
                    RebindSceneReferences();
                    JoinRequest request = JsonUtility.FromJson<JoinRequest>(payload);
                    if (request == null || string.IsNullOrWhiteSpace(request.ViewerId))
                    {
                        return;
                    }

                    if (sessionService != null &&
                        !string.IsNullOrWhiteSpace(sessionService.SessionId) &&
                        !string.Equals(request.SessionId, sessionService.SessionId, StringComparison.OrdinalIgnoreCase))
                    {
                        SendAsync(client, "notice|invalid_session");
                        _ = CloseClientAsync(client);
                        return;
                    }

                    client.ViewerId = request.ViewerId;
                    Debug.Log($"Viewer TCP join accepted: {client.ViewerId}");
                    EnsureBudgetInitialized();
                    EnsureJoinReward(request.ViewerId);
                    SendAsync(client, "notice|joined");
                    SendAsync(client, $"snapshot|{JsonUtility.ToJson(BuildSnapshot(client.ViewerId))}");
                    break;
                }
                case "command":
                {
                    RebindSceneReferences();
                    PurchaseRequest request = JsonUtility.FromJson<PurchaseRequest>(payload);
                    if (request == null)
                    {
                        return;
                    }

                    if (actionExecutor == null)
                    {
                        Debug.LogWarning($"Viewer command dropped because host action executor is missing. Action={request.ActionId}, Viewer={request.ViewerId}");
                        SendAsync(client, "notice|host_not_ready");
                        return;
                    }

                    Debug.Log($"Viewer command received: viewer={request.ViewerId}, action={request.ActionId}, target={request.TargetId}, viewport=({request.ViewportX:0.000},{request.ViewportY:0.000})");
                    ViewerCommand command = new(request.CommandId, request.ViewerId, request.ActionId, request.TargetId, request.HasViewportTarget, request.ViewportX, request.ViewportY);
                    actionExecutor.TrySubmit(command);
                    break;
                }
            }
        }

        private ViewerSessionSnapshot BuildSnapshot(string viewerId = "")
        {
            RebindSceneReferences();
            EnsureBudgetInitialized();
            return new ViewerSessionSnapshot
            {
                SessionId = sessionService != null ? sessionService.SessionId : "local-session",
                ViewerId = viewerId,
                RoomIndex = runController != null ? runController.RoomIndex : 0,
                RoomPhase = runController != null ? runController.CurrentPhase : "explore",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                HostShield = playerHealth != null ? playerHealth.MaxShield : 0f,
                AliveEnemyCount = runController != null ? runController.TotalAliveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = roomBudgetService != null ? roomBudgetService.CurrentBudget : 0,
                ViewerBalance = economyService != null && !string.IsNullOrWhiteSpace(viewerId) ? economyService.GetBalance(viewerId) : 0,
                PlayerMarker = BuildPlayerMarker(),
                TargetMarker = BuildTargetMarker(),
                EnemyMarkers = BuildEnemyMarkers()
            };
        }

        private void BroadcastSnapshot(ViewerSessionSnapshot snapshot)
        {
            List<ClientConnection> snapshotClients = SnapshotClients();
            for (int i = 0; i < snapshotClients.Count; i++)
            {
                ClientConnection client = snapshotClients[i];
                ViewerSessionSnapshot perViewerSnapshot = new()
                {
                    SessionId = snapshot.SessionId,
                    ViewerId = client.ViewerId,
                    RoomIndex = snapshot.RoomIndex,
                    RoomPhase = snapshot.RoomPhase,
                    HostHp = snapshot.HostHp,
                    HostShield = snapshot.HostShield,
                    AliveEnemyCount = snapshot.AliveEnemyCount,
                    QueueCount = snapshot.QueueCount,
                    RoomBudget = snapshot.RoomBudget,
                    ViewerBalance = economyService != null && !string.IsNullOrWhiteSpace(client.ViewerId) ? economyService.GetBalance(client.ViewerId) : 0,
                    PlayerMarker = snapshot.PlayerMarker,
                    TargetMarker = snapshot.TargetMarker,
                    EnemyMarkers = snapshot.EnemyMarkers
                };
                SendAsync(client, $"snapshot|{JsonUtility.ToJson(perViewerSnapshot)}");
            }
        }

        private void BroadcastDelta(ViewerSessionDelta delta)
        {
            List<ClientConnection> snapshotClients = SnapshotClients();
            for (int i = 0; i < snapshotClients.Count; i++)
            {
                ClientConnection client = snapshotClients[i];
                ViewerSessionDelta perViewerDelta = new()
                {
                    EventType = delta.EventType,
                    ViewerId = client.ViewerId,
                    RoomPhase = delta.RoomPhase,
                    HostHp = delta.HostHp,
                    AliveEnemyCount = delta.AliveEnemyCount,
                    QueueCount = delta.QueueCount,
                    RoomBudget = delta.RoomBudget,
                    ViewerBalance = economyService != null && !string.IsNullOrWhiteSpace(client.ViewerId) ? economyService.GetBalance(client.ViewerId) : 0,
                    PlayerMarker = delta.PlayerMarker,
                    TargetMarker = delta.TargetMarker,
                    EnemyMarkers = delta.EnemyMarkers
                };
                SendAsync(client, $"delta|{JsonUtility.ToJson(perViewerDelta)}");
            }
        }

        private void BroadcastResult(ViewerActionResult result)
        {
            BroadcastToAll($"result|{JsonUtility.ToJson(result)}");
        }

        private void BroadcastToAll(string message)
        {
            List<ClientConnection> snapshot = SnapshotClients();
            for (int i = 0; i < snapshot.Count; i++)
            {
                SendAsync(snapshot[i], message);
            }
        }

        private void GrantJoinRewardsForConnectedViewers()
        {
            List<ClientConnection> snapshot = SnapshotClients();
            for (int i = 0; i < snapshot.Count; i++)
            {
                EnsureJoinReward(snapshot[i].ViewerId);
            }
        }

        private void EnsureJoinReward(string viewerId)
        {
            if (economyService == null || string.IsNullOrWhiteSpace(viewerId))
            {
                return;
            }

            int currentBalance = economyService.GetBalance(viewerId);
            if (rewardedViewerIds.Contains(viewerId) && currentBalance > 0)
            {
                return;
            }

            if (currentBalance <= 0)
            {
                economyService.AddCurrency(viewerId, Mathf.Max(1, debugTopUpBalance));
            }
            else
            {
                economyService.GrantJoinReward(viewerId);
            }

            rewardedViewerIds.Add(viewerId);
        }

        private void EnsureBudgetInitialized()
        {
            if (roomBudgetService != null && roomBudgetService.CurrentBudget <= 0)
            {
                roomBudgetService.ResetBudget();
            }
        }

        private ViewerViewportMarkerState BuildPlayerMarker()
        {
            if (playerHealth == null)
            {
                return new ViewerViewportMarkerState { Id = "player", Kind = "player", Visible = false };
            }

            return BuildMarker("player", "player", playerHealth.transform.position);
        }

        private ViewerViewportMarkerState BuildTargetMarker()
        {
            RunFinishGate gate = FindFirstObjectByType<RunFinishGate>();
            if (gate == null)
            {
                return new ViewerViewportMarkerState { Id = "target", Kind = "target", Visible = false };
            }

            return BuildMarker("target", "target", gate.transform.position);
        }

        private ViewerViewportMarkerState[] BuildEnemyMarkers()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0)
            {
                return Array.Empty<ViewerViewportMarkerState>();
            }

            ViewerViewportMarkerState[] markers = new ViewerViewportMarkerState[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyController enemy = enemies[i];
                markers[i] = enemy != null
                    ? BuildMarker($"enemy_{i:00}", "enemy", enemy.transform.position)
                    : new ViewerViewportMarkerState { Id = $"enemy_{i:00}", Kind = "enemy", Visible = false };
            }

            return markers;
        }

        private static ViewerViewportMarkerState BuildMarker(string id, string kind, Vector3 worldPosition)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return new ViewerViewportMarkerState { Id = id, Kind = kind, Visible = false };
            }

            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            bool visible = viewport.z >= 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
            return new ViewerViewportMarkerState
            {
                Id = id,
                Kind = kind,
                ViewportX = Mathf.Clamp01(viewport.x),
                ViewportY = Mathf.Clamp01(viewport.y),
                Visible = visible
            };
        }

        private List<ClientConnection> SnapshotClients()
        {
            lock (clientsLock)
            {
                return new List<ClientConnection>(clients);
            }
        }

        private void SendAsync(ClientConnection client, string message)
        {
            if (client == null || client.Writer == null)
            {
                return;
            }

            try
            {
                lock (client.WriteLock)
                {
                    client.Writer.WriteLine(message);
                    client.Writer.Flush();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Host TCP send failed for {client.ViewerId}: {exception.Message}");
                _ = CloseClientAsync(client);
            }
        }

        private Task CloseClientAsync(ClientConnection client)
        {
            if (client == null)
            {
                return Task.CompletedTask;
            }

            lock (clientsLock)
            {
                clients.Remove(client);
            }

            try { client.Writer?.Dispose(); } catch { }
            try { client.Reader?.Dispose(); } catch { }
            try { client.Client?.Close(); client.Client?.Dispose(); } catch { }
            return Task.CompletedTask;
        }
    }
}
