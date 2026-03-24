using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoulForge.Bootstrap;
using SoulForge.Economy;
using SoulForge.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulForge.Viewer
{
    public sealed class HostViewerWebSocketServer : MonoBehaviour
    {
        private static HostViewerWebSocketServer instance;

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
        }

        private sealed class ClientConnection
        {
            public string ViewerId;
            public WebSocket Socket;
        }

        [SerializeField] private int port = 8080;
        [SerializeField] private string route = "ws";
        [SerializeField] private string listenPrefix = "http://+:8080/ws/";
        [SerializeField] private ViewerSessionService sessionService;
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerActionExecutor actionExecutor;
        [SerializeField] private ViewerEconomyService economyService;
        [SerializeField] private RunController runController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ViewerActionQueue viewerActionQueue;
        [SerializeField] private ViewerRoomBudgetService roomBudgetService;

        private readonly ConcurrentQueue<Action> mainThreadActions = new();
        private readonly List<ClientConnection> clients = new();
        private readonly object clientsLock = new();

        private CancellationTokenSource cancellation;
        private HttpListener listener;

        public static HostViewerWebSocketServer Instance => instance;
        public string ListenPrefix => listenPrefix;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
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

            if (string.IsNullOrWhiteSpace(listenPrefix))
            {
                listenPrefix = $"http://+:{port}/{route.Trim('/')}/";
            }

            cancellation = new CancellationTokenSource();
            listener = new HttpListener();
            listener.Prefixes.Add(listenPrefix);

            try
            {
                listener.Start();
                _ = AcceptLoopAsync(cancellation.Token);
                Debug.Log($"Host viewer websocket server listening on {listenPrefix}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to start host websocket server on {listenPrefix}: {exception.Message}");
                listener.Close();
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
            listener.Close();
            listener = null;
            cancellation?.Dispose();
            cancellation = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebindSceneReferences();
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
            while (!token.IsCancellationRequested && listener != null && listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    continue;
                }

                try
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    ClientConnection client = new() { Socket = webSocketContext.WebSocket };

                    lock (clientsLock)
                    {
                        clients.Add(client);
                    }

                    _ = ReceiveLoopAsync(client, token);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Failed to accept websocket client: {exception.Message}");
                }
            }
        }

        private async Task ReceiveLoopAsync(ClientConnection client, CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested && client.Socket != null && client.Socket.State == WebSocketState.Open)
                {
                    StringBuilder builder = new();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await CloseClientAsync(client);
                            return;
                        }

                        builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    string message = builder.ToString();
                    mainThreadActions.Enqueue(() => ProcessIncoming(client, message));
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Viewer client disconnected: {exception.Message}");
            }
            finally
            {
                await CloseClientAsync(client);
            }
        }

        private void ProcessIncoming(ClientConnection client, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            int separatorIndex = message.IndexOf('|');
            if (separatorIndex <= 0)
            {
                return;
            }

            string type = message[..separatorIndex];
            string payload = message[(separatorIndex + 1)..];

            switch (type)
            {
                case "join":
                {
                    JoinRequest request = JsonUtility.FromJson<JoinRequest>(payload);
                    if (request == null || string.IsNullOrWhiteSpace(request.ViewerId))
                    {
                        return;
                    }

                    client.ViewerId = request.ViewerId;
                    economyService?.GrantJoinReward(request.ViewerId);
                    SendAsync(client, "notice|joined");
                    SendAsync(client, $"snapshot|{JsonUtility.ToJson(BuildSnapshot())}");
                    break;
                }
                case "command":
                {
                    PurchaseRequest request = JsonUtility.FromJson<PurchaseRequest>(payload);
                    if (request == null || actionExecutor == null)
                    {
                        return;
                    }

                    ViewerCommand command = new(request.CommandId, request.ViewerId, request.ActionId, request.TargetId);
                    actionExecutor.TrySubmit(command);
                    break;
                }
            }
        }

        private ViewerSessionSnapshot BuildSnapshot()
        {
            return new ViewerSessionSnapshot
            {
                SessionId = sessionService != null ? sessionService.SessionId : "local-session",
                RoomIndex = runController != null ? runController.RoomIndex : 0,
                RoomPhase = runController != null && runController.CurrentRoom != null && runController.CurrentRoom.IsLocked ? "combat" : "reward",
                HostHp = playerHealth != null ? playerHealth.CurrentHealth : 0f,
                HostShield = playerHealth != null ? playerHealth.MaxShield : 0f,
                AliveEnemyCount = runController != null && runController.CurrentRoom != null ? runController.CurrentRoom.ActiveEnemyCount : 0,
                QueueCount = viewerActionQueue != null ? viewerActionQueue.Count : 0,
                RoomBudget = roomBudgetService != null ? roomBudgetService.CurrentBudget : 0
            };
        }

        private void BroadcastSnapshot(ViewerSessionSnapshot snapshot)
        {
            BroadcastToAll($"snapshot|{JsonUtility.ToJson(snapshot)}");
        }

        private void BroadcastDelta(ViewerSessionDelta delta)
        {
            BroadcastToAll($"delta|{JsonUtility.ToJson(delta)}");
        }

        private void BroadcastResult(ViewerActionResult result)
        {
            BroadcastToAll($"result|{JsonUtility.ToJson(result)}");
        }

        private void BroadcastToAll(string message)
        {
            List<ClientConnection> snapshot;
            lock (clientsLock)
            {
                snapshot = new List<ClientConnection>(clients);
            }

            for (int i = 0; i < snapshot.Count; i++)
            {
                SendAsync(snapshot[i], message);
            }
        }

        private async void SendAsync(ClientConnection client, string message)
        {
            if (client == null || client.Socket == null || client.Socket.State != WebSocketState.Open)
            {
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                await client.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                await CloseClientAsync(client);
            }
        }

        private async Task CloseClientAsync(ClientConnection client)
        {
            if (client == null)
            {
                return;
            }

            lock (clientsLock)
            {
                clients.Remove(client);
            }

            if (client.Socket == null)
            {
                return;
            }

            try
            {
                if (client.Socket.State == WebSocketState.Open || client.Socket.State == WebSocketState.CloseReceived)
                {
                    await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None);
                }
            }
            catch
            {
            }
            finally
            {
                client.Socket.Dispose();
            }
        }
    }
}
