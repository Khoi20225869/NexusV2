using System;

namespace SoulForge.Viewer
{
    [Serializable]
    public sealed class ViewerViewportMarkerState
    {
        public string Id;
        public string Kind;
        public float ViewportX;
        public float ViewportY;
        public bool Visible;
    }
}
