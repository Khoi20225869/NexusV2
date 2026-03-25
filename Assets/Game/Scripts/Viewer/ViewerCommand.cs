namespace SoulForge.Viewer
{
    public readonly struct ViewerCommand
    {
        public ViewerCommand(string commandId, string viewerId, string actionId, string targetId, bool hasViewportTarget = false, float viewportX = 0f, float viewportY = 0f)
        {
            CommandId = commandId;
            ViewerId = viewerId;
            ActionId = actionId;
            TargetId = targetId;
            HasViewportTarget = hasViewportTarget;
            ViewportX = viewportX;
            ViewportY = viewportY;
        }

        public string CommandId { get; }
        public string ViewerId { get; }
        public string ActionId { get; }
        public string TargetId { get; }
        public bool HasViewportTarget { get; }
        public float ViewportX { get; }
        public float ViewportY { get; }
    }
}
