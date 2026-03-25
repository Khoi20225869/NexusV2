using UnityEngine;

namespace SoulForge.Viewer
{
    public sealed class ViewerSessionService : MonoBehaviour
    {
        private static ViewerSessionService instance;

        [SerializeField] private string sessionId = "";
        [SerializeField] private bool autoCopyToClipboard = true;

        public string SessionId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    sessionId = GenerateSessionCode();
                }

                return sessionId;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            _ = SessionId;
            CopySessionCodeIfEnabled();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        [ContextMenu("Regenerate Session Code")]
        public void RegenerateSessionCode()
        {
            sessionId = GenerateSessionCode();
            CopySessionCodeIfEnabled();
        }

        private static string GenerateSessionCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] buffer = new char[6];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = chars[Random.Range(0, chars.Length)];
            }

            return new string(buffer);
        }

        private void CopySessionCodeIfEnabled()
        {
            if (!autoCopyToClipboard || string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            GUIUtility.systemCopyBuffer = sessionId;
            Debug.Log($"Session code copied to clipboard: {sessionId}");
        }
    }
}
