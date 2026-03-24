namespace SoulForge.Viewer
{
    public readonly struct ViewerCommand
    {
        public ViewerCommand(string commandId, string viewerId, string actionId, string targetId)
        {
            CommandId = commandId;
            ViewerId = viewerId;
            ActionId = actionId;
            TargetId = targetId;
        }

        public string CommandId { get; }
        public string ViewerId { get; }
        public string ActionId { get; }
        public string TargetId { get; }
    }
}
