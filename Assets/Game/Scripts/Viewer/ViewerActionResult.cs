using System;

namespace SoulForge.Viewer
{
    [Serializable]
    public sealed class ViewerActionResult
    {
        public string CommandId;
        public string ViewerId;
        public bool Success;
        public string Reason;
        public string ActionId;
    }
}
