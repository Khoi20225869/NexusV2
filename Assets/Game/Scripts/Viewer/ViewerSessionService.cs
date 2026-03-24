using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerSessionService : MonoBehaviour
    {
        [SerializeField] private string sessionId = "local-session";

        public string SessionId => sessionId;
    }
}
