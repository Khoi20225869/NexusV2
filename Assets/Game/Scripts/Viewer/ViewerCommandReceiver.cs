using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Viewer
{
    public sealed class ViewerCommandReceiver : MonoBehaviour
    {
        [System.Serializable]
        public sealed class ViewerCommandEvent : UnityEvent<string, string, string, string> { }

        [SerializeField] private ViewerCommandEvent onCommandReceived;

        public void Receive(string commandId, string viewerId, string actionId, string targetId = "")
        {
            onCommandReceived?.Invoke(commandId, viewerId, actionId, targetId);
        }
    }
}
