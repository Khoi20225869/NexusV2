using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerWebSocketClient : MonoBehaviour
    {
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
        [SerializeField] private string route = "ws";
        [SerializeField] private string viewerId = "viewer_01";
        [SerializeField] private string sessionId = "local-session";
        [SerializeField] private bool autoConnect = true;
        [SerializeField] private StateBroadcaster stateBroadcaster;

        private readonly ConcurrentQueue<string> incomingMessages = new();
        private ClientWebSocket socket;
        private CancellationTokenSource cancellation;
        private int commandCounter;

        public string ViewerId => viewerId;
        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;
        public string Url => $"ws://{hostIp}:{port}/{route.Trim('/')}/";

        private void Awake()
        {
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

            cancellation = new CancellationTokenSource();
            socket = new ClientWebSocket();

            try
            {
                await socket.ConnectAsync(new Uri(Url), cancellation.Token);
                Debug.Log($"Viewer websocket connected to {Url}");
                SendJoin();
                _ = ReceiveLoopAsync(cancellation.Token);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Viewer websocket connect failed to {Url}: {exception.Message}. Check host Play mode, host IP, port, and firewall.");
                Disconnect();
            }
        }

        [ContextMenu("Disconnect")]
        public async void Disconnect()
        {
            cancellation?.Cancel();

            if (socket == null)
            {
                cancellation?.Dispose();
                cancellation = null;
                return;
            }

            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "disconnect", CancellationToken.None);
                }
            }
            catch
            {
            }
            finally
            {
                socket.Dispose();
                socket = null;
                cancellation?.Dispose();
                cancellation = null;
            }
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
            if (!IsConnected)
            {
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Viewer websocket send failed: {exception.Message}");
                Disconnect();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
                {
                    StringBuilder builder = new();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Disconnect();
                            return;
                        }

                        builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    incomingMessages.Enqueue(builder.ToString());
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Viewer websocket receive failed: {exception.Message}");
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

            switch (type)
            {
                case "snapshot":
                    stateBroadcaster?.BroadcastSnapshot(JsonUtility.FromJson<ViewerSessionSnapshot>(payload));
                    break;
                case "delta":
                    stateBroadcaster?.BroadcastDelta(JsonUtility.FromJson<ViewerSessionDelta>(payload));
                    break;
                case "result":
                    stateBroadcaster?.BroadcastActionResolved(JsonUtility.FromJson<ViewerActionResult>(payload));
                    break;
                case "notice":
                    Debug.Log($"Viewer notice: {payload}");
                    break;
            }
        }
    }
}
