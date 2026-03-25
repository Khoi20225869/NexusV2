using SoulForge.Viewer;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerLiveHudPresenter : MonoBehaviour
    {
        [SerializeField] private StateBroadcaster stateBroadcaster;
        [SerializeField] private ViewerWebSocketClient viewerWebSocketClient;
        [SerializeField] private TMP_Text connectionText;
        [SerializeField] private TMP_Text floorText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text hpText;

        private ViewerSessionSnapshot snapshot;
        private ViewerSessionDelta delta;

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
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast += OnSnapshot;
                stateBroadcaster.DeltaBroadcast += OnDelta;
            }

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.StatusChanged += OnStatusChanged;
                OnStatusChanged(viewerWebSocketClient.LastStatus);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (stateBroadcaster != null)
            {
                stateBroadcaster.SnapshotBroadcast -= OnSnapshot;
                stateBroadcaster.DeltaBroadcast -= OnDelta;
            }

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.StatusChanged -= OnStatusChanged;
            }
        }

        private void OnSnapshot(ViewerSessionSnapshot nextSnapshot)
        {
            snapshot = nextSnapshot;
            Refresh();
        }

        private void OnDelta(ViewerSessionDelta nextDelta)
        {
            delta = nextDelta;
            Refresh();
        }

        private void OnStatusChanged(string status)
        {
            if (connectionText != null)
            {
                connectionText.text = $"Status: {status}";
            }
        }

        private void Refresh()
        {
            if (floorText != null)
            {
                floorText.text = $"Floor {((snapshot != null && snapshot.RoomIndex > 0) ? snapshot.RoomIndex : 1)}";
            }

            if (phaseText != null)
            {
                string phase = !string.IsNullOrWhiteSpace(delta?.RoomPhase) ? delta.RoomPhase : snapshot != null ? snapshot.RoomPhase : "offline";
                phaseText.text = $"Phase: {phase}";
            }

            if (hpText != null)
            {
                float hp = delta != null && delta.HostHp > 0f ? delta.HostHp : snapshot != null ? snapshot.HostHp : 0f;
                hpText.text = $"Host HP: {hp:0.0}";
            }
        }
    }
}
