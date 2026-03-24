using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Viewer
{
    public sealed class StateBroadcaster : MonoBehaviour
    {
        [System.Serializable]
        public sealed class MessageEvent : UnityEvent<string> { }

        [System.Serializable]
        public sealed class SnapshotEvent : UnityEvent<ViewerSessionSnapshot> { }

        [System.Serializable]
        public sealed class DeltaEvent : UnityEvent<ViewerSessionDelta> { }

        [System.Serializable]
        public sealed class ResultEvent : UnityEvent<ViewerActionResult> { }

        [SerializeField] private MessageEvent onBroadcast;
        [SerializeField] private SnapshotEvent onSnapshotBroadcast;
        [SerializeField] private DeltaEvent onDeltaBroadcast;
        [SerializeField] private ResultEvent onActionResultBroadcast;

        public event System.Action<ViewerSessionSnapshot> SnapshotBroadcast;
        public event System.Action<ViewerSessionDelta> DeltaBroadcast;
        public event System.Action<ViewerActionResult> ActionResultBroadcast;
        public event System.Action<string> JsonBroadcast;

        public void BroadcastSnapshot(ViewerSessionSnapshot payload)
        {
            SnapshotBroadcast?.Invoke(payload);
            onSnapshotBroadcast?.Invoke(payload);
            string json = JsonUtility.ToJson(payload);
            JsonBroadcast?.Invoke(json);
            onBroadcast?.Invoke(json);
        }

        public void BroadcastDelta(ViewerSessionDelta payload)
        {
            DeltaBroadcast?.Invoke(payload);
            onDeltaBroadcast?.Invoke(payload);
            string json = JsonUtility.ToJson(payload);
            JsonBroadcast?.Invoke(json);
            onBroadcast?.Invoke(json);
        }

        public void BroadcastActionResolved(ViewerActionResult payload)
        {
            ActionResultBroadcast?.Invoke(payload);
            onActionResultBroadcast?.Invoke(payload);
            string json = JsonUtility.ToJson(payload);
            JsonBroadcast?.Invoke(json);
            onBroadcast?.Invoke(json);
        }
    }
}
