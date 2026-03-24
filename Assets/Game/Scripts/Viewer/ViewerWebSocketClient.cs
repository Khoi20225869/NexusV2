using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerWebSocketClient : MonoBehaviour
    {
        private static readonly UTF8Encoding Utf8NoBom = new(false);
        private const string ViewerIdPrefsKey = "SoulForge.ViewerId";
        private const string HostIpPrefsKey = "SoulForge.LastHostIp";
        private const string SessionCodePrefsKey = "SoulForge.LastSessionCode";
        private const string RecentSessionsPrefsKey = "SoulForge.RecentSessions";

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

        [SerializeField] private string hostIp = "127.0.0.1";
        [SerializeField] private int port = 8080;
        [SerializeField] private string viewerId = "viewer_01";
        [SerializeField] private string sessionId = "local-session";
        [SerializeField] private bool autoConnect = true;
        [SerializeField] private StateBroadcaster stateBroadcaster;

        private readonly ConcurrentQueue<string> incomingMessages = new();
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        private CancellationTokenSource cancellation;
        private int commandCounter;
        private string lastStatus = "Offline";
        private bool isDisconnecting;
        private string lastSuccessfulSession;

        public string ViewerId => viewerId;
        public bool IsConnected => tcpClient != null && tcpClient.Connected;
        public string HostIp => hostIp;
        public string SessionCode => sessionId;
        public string LastStatus => lastStatus;
        public string Url => $"tcp://{hostIp}:{port}";
        public string LastSuccessfulSession => lastSuccessfulSession;

        public event Action<bool> ConnectionStateChanged;
        public event Action<string> StatusChanged;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(viewerId) || viewerId == "viewer_01")
            {
                viewerId = GetOrCreateViewerId();
            }

            hostIp = PlayerPrefs.GetString(HostIpPrefsKey, hostIp);
            sessionId = PlayerPrefs.GetString(SessionCodePrefsKey, sessionId);
            lastSuccessfulSession = BuildSessionLabel(hostIp, sessionId);

            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }
        }

        private void Start()
        {
            if (autoConnect)
            {
                Connect();
            }
        }

        private void Update()
        {
            while (incomingMessages.TryDequeue(out string message))
            {
                ProcessIncoming(message);
            }
        }

        private void OnDisable()
        {
            Disconnect();
        }

        [ContextMenu("Connect")]
        public async void Connect()
        {
            if (IsConnected)
            {
                return;
            }

            SetStatus($"Connecting to {Url}");
            cancellation = new CancellationTokenSource();
            tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(hostIp, port);
                NetworkStream stream = tcpClient.GetStream();
                tcpClient.NoDelay = true;
                reader = new StreamReader(stream, Utf8NoBom);
                writer = new StreamWriter(stream, Utf8NoBom) { AutoFlush = true };
                Debug.Log($"Viewer TCP connected to {Url}");
                SetStatus($"Connected to {Url}");
                ConnectionStateChanged?.Invoke(true);
                SendJoin();
                _ = ReceiveLoopAsync(cancellation.Token);
            }
            catch (Exception exception)
            {
                string message = $"Viewer TCP connect failed to {Url}: {exception.Message}. Check host Play mode, host IP, port, and firewall.";
                Debug.LogError(message);
                SetStatus($"Offline: {exception.Message}");
                ConnectionStateChanged?.Invoke(false);
                Disconnect();
            }
        }

        [ContextMenu("Disconnect")]
        public void Disconnect()
        {
            if (isDisconnecting)
            {
                return;
            }

            isDisconnecting = true;
            cancellation?.Cancel();

            try { writer?.Dispose(); } catch { }
            try { reader?.Dispose(); } catch { }
            try { tcpClient?.Close(); tcpClient?.Dispose(); } catch { }

            writer = null;
            reader = null;
            tcpClient = null;
            cancellation?.Dispose();
            cancellation = null;
            SetStatus("Offline");
            ConnectionStateChanged?.Invoke(false);
            isDisconnecting = false;
        }

        public void SetHostIp(string nextHostIp)
        {
            if (string.IsNullOrWhiteSpace(nextHostIp))
            {
                return;
            }

            hostIp = nextHostIp.Trim();
            PlayerPrefs.SetString(HostIpPrefsKey, hostIp);
            SetStatus($"Host IP set to {hostIp}");
        }

        public void SetSessionCode(string nextSessionCode)
        {
            if (string.IsNullOrWhiteSpace(nextSessionCode))
            {
                return;
            }

            sessionId = nextSessionCode.Trim().ToUpperInvariant();
            PlayerPrefs.SetString(SessionCodePrefsKey, sessionId);
            SetStatus($"Session set to {sessionId}");
        }

        public void ReconnectLast()
        {
            if (string.IsNullOrWhiteSpace(hostIp) || string.IsNullOrWhiteSpace(sessionId))
            {
                SetStatus("No recent session");
                return;
            }

            Disconnect();
            Connect();
        }

        public string GetRecentSessionsDisplay()
        {
            string raw = PlayerPrefs.GetString(RecentSessionsPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "Recent Sessions\nNone yet";
            }

            string[] sessions = raw.Split('\n');
            StringBuilder builder = new("Recent Sessions\n");
            for (int i = 0; i < sessions.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sessions[i]))
                {
                    builder.Append(i + 1).Append(". ").Append(sessions[i]).Append('\n');
                }
            }

            return builder.ToString().TrimEnd();
        }

        public void SubmitAction(string actionId, string targetId = "")
        {
            if (!IsConnected)
            {
                return;
            }

            commandCounter++;
            PurchaseRequest payload = new()
            {
                CommandId = $"net_cmd_{commandCounter:0000}",
                ViewerId = viewerId,
                ActionId = actionId,
                TargetId = targetId
            };

            SendRaw($"command|{JsonUtility.ToJson(payload)}");
        }

        private void SendJoin()
        {
            JoinRequest payload = new()
            {
                ViewerId = viewerId,
                SessionId = sessionId
            };

            SendRaw($"join|{JsonUtility.ToJson(payload)}");
        }

        private async void SendRaw(string message)
        {
            if (!IsConnected || writer == null)
            {
                return;
            }

            try
            {
                await writer.WriteLineAsync(message);
            }
            catch (Exception exception)
            {
                if (!isDisconnecting)
                {
                    Debug.LogWarning($"Viewer TCP send failed: {exception.Message}");
                }

                Disconnect();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && reader != null)
                {
                    string message = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Debug.Log("Viewer TCP server closed the stream.");
                        Disconnect();
                        return;
                    }

                    incomingMessages.Enqueue(message);
                }
            }
            catch (Exception exception)
            {
                if (!isDisconnecting && !token.IsCancellationRequested)
                {
                    Debug.LogWarning($"Viewer TCP receive failed: {exception.Message}");
                }
            }
        }

        private void ProcessIncoming(string message)
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
            type = type.TrimStart('\uFEFF');

            switch (type)
            {
                case "snapshot":
                    ViewerSessionSnapshot snapshot = JsonUtility.FromJson<ViewerSessionSnapshot>(payload);
                    stateBroadcaster?.BroadcastSnapshot(snapshot);
                    SetStatus($"Connected to {hostIp}");
                    break;
                case "delta":
                    stateBroadcaster?.BroadcastDelta(JsonUtility.FromJson<ViewerSessionDelta>(payload));
                    break;
                case "result":
                    stateBroadcaster?.BroadcastActionResolved(JsonUtility.FromJson<ViewerActionResult>(payload));
                    break;
                case "notice":
                    Debug.Log($"Viewer notice: {payload}");
                    if (payload == "joined")
                    {
                        RememberSuccessfulSession();
                    }
                    SetStatus(payload);
                    break;
            }
        }

        private void SetStatus(string status)
        {
            lastStatus = status;
            StatusChanged?.Invoke(lastStatus);
        }

        private static string GetOrCreateViewerId()
        {
            string saved = PlayerPrefs.GetString(ViewerIdPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(saved))
            {
                return saved;
            }

            string generated = $"viewer_{UnityEngine.Random.Range(1000, 9999)}";
            PlayerPrefs.SetString(ViewerIdPrefsKey, generated);
            PlayerPrefs.Save();
            return generated;
        }

        private void RememberSuccessfulSession()
        {
            PlayerPrefs.SetString(HostIpPrefsKey, hostIp);
            PlayerPrefs.SetString(SessionCodePrefsKey, sessionId);

            string entry = BuildSessionLabel(hostIp, sessionId);
            lastSuccessfulSession = entry;
            string[] previous = PlayerPrefs.GetString(RecentSessionsPrefsKey, string.Empty).Split('\n');
            StringBuilder builder = new();
            builder.AppendLine(entry);

            int added = 1;
            for (int i = 0; i < previous.Length && added < 5; i++)
            {
                string existing = previous[i].Trim();
                if (string.IsNullOrWhiteSpace(existing) || string.Equals(existing, entry, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                builder.AppendLine(existing);
                added++;
            }

            PlayerPrefs.SetString(RecentSessionsPrefsKey, builder.ToString().TrimEnd());
            PlayerPrefs.Save();
        }

        private static string BuildSessionLabel(string ip, string code)
        {
            return $"{ip}  [{code}]";
        }
    }
}
