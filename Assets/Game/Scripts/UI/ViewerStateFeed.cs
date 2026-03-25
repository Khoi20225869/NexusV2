using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerStateFeed : MonoBehaviour
    {
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerWebSocketClient viewerWebSocketClient;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text connectionText;
        [SerializeField] private TMP_Text snapshotText;
        [SerializeField] private TMP_Text deltaText;
        [SerializeField] private TMP_Text actionResultText;
        [SerializeField] private TMP_Text rawJsonText;

        private ViewerSessionSnapshot latestSnapshot;
        private ViewerSessionDelta latestDelta;

        private void Awake()
        {
            if (stateBroadcaster == null)
            {
                stateBroadcaster = FindFirstObjectByType<StateBroadcaster>();
            }

            if (viewerWebSocketClient == null)
            {
                viewerWebSocketClient = FindFirstObjectByType<ViewerWebSocketClient>();
            }
        }

        private void OnEnable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.SnapshotBroadcast += ShowSnapshot;
            stateBroadcaster.DeltaBroadcast += ShowDelta;
            stateBroadcaster.ActionResultBroadcast += ShowActionResult;
            stateBroadcaster.JsonBroadcast += ShowRawJson;

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.ConnectionStateChanged += OnConnectionStateChanged;
                viewerWebSocketClient.StatusChanged += OnStatusChanged;
                OnConnectionStateChanged(viewerWebSocketClient.IsConnected);
                OnStatusChanged(viewerWebSocketClient.LastStatus);
            }
        }

        private void OnDisable()
        {
            if (stateBroadcaster == null)
            {
                return;
            }

            stateBroadcaster.SnapshotBroadcast -= ShowSnapshot;
            stateBroadcaster.DeltaBroadcast -= ShowDelta;
            stateBroadcaster.ActionResultBroadcast -= ShowActionResult;
            stateBroadcaster.JsonBroadcast -= ShowRawJson;

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.ConnectionStateChanged -= OnConnectionStateChanged;
                viewerWebSocketClient.StatusChanged -= OnStatusChanged;
            }
        }

        public void ShowSnapshot(ViewerSessionSnapshot snapshot)
        {
            latestSnapshot = snapshot;
            if (snapshotText == null)
            {
                RefreshSummary();
                return;
            }

            snapshotText.text =
                $"Session: {snapshot.SessionId}\n" +
                $"Viewer: {snapshot.ViewerId}\n" +
                $"Floor: {snapshot.RoomIndex}\n" +
                $"Phase: {snapshot.RoomPhase}\n" +
                $"HP: {snapshot.HostHp:0.0}\n" +
                $"Shield: {snapshot.HostShield:0.0}\n" +
                $"Enemies: {snapshot.AliveEnemyCount}\n" +
                $"Queue: {snapshot.QueueCount}\n" +
                $"Budget: {snapshot.RoomBudget}\n" +
                $"Crowns: {snapshot.ViewerBalance}";
            RefreshSummary();
        }

        public void ShowDelta(ViewerSessionDelta delta)
        {
            latestDelta = delta;
            if (deltaText == null)
            {
                RefreshSummary();
                return;
            }

            deltaText.text =
                $"Event: {delta.EventType}\n" +
                $"Viewer: {delta.ViewerId}\n" +
                $"Phase: {delta.RoomPhase}\n" +
                $"HP: {delta.HostHp:0.0}\n" +
                $"Enemies: {delta.AliveEnemyCount}\n" +
                $"Queue: {delta.QueueCount}\n" +
                $"Budget: {delta.RoomBudget}\n" +
                $"Crowns: {delta.ViewerBalance}";
            RefreshSummary();
        }

        public void ShowActionResult(ViewerActionResult result)
        {
            if (actionResultText == null)
            {
                return;
            }

            actionResultText.text =
                $"Viewer: {result.ViewerId}\n" +
                $"Command: {result.CommandId}\n" +
                $"Action: {result.ActionId}\n" +
                $"Success: {result.Success}\n" +
                $"Reason: {result.Reason}";
        }

        public void ShowRawJson(string payload)
        {
            if (rawJsonText == null)
            {
                return;
            }

            rawJsonText.text = payload;
        }

        private void OnConnectionStateChanged(bool isConnected)
        {
            if (connectionText != null)
            {
                connectionText.text = $"Connection: {(isConnected ? "Connected" : "Offline")}";
            }

            RefreshSummary();
        }

        private void OnStatusChanged(string status)
        {
            if (connectionText != null && !string.IsNullOrWhiteSpace(status))
            {
                connectionText.text = $"Connection: {status}";
            }
        }

        private void RefreshSummary()
        {
            if (summaryText == null)
            {
                return;
            }

            int roomIndex = latestSnapshot != null ? latestSnapshot.RoomIndex : 0;
            float hp = latestDelta != null && latestDelta.HostHp > 0f ? latestDelta.HostHp : latestSnapshot != null ? latestSnapshot.HostHp : 0f;
            int budget = latestDelta != null && latestDelta.RoomBudget > 0 ? latestDelta.RoomBudget : latestSnapshot != null ? latestSnapshot.RoomBudget : 0;
            int queue = latestDelta != null ? latestDelta.QueueCount : latestSnapshot != null ? latestSnapshot.QueueCount : 0;
            int balance = latestDelta != null && latestDelta.ViewerBalance > 0 ? latestDelta.ViewerBalance : latestSnapshot != null ? latestSnapshot.ViewerBalance : 0;
            bool isConnected = viewerWebSocketClient != null && viewerWebSocketClient.IsConnected;

            summaryText.text =
                $"Status: {(isConnected ? "Connected" : "Offline")}\n" +
                $"Floor: {roomIndex}\n" +
                $"HP: {hp:0.0}\n" +
                $"Crowns: {balance}\n" +
                $"Budget: {budget}\n" +
                $"Queue: {queue}";
        }
    }
}
