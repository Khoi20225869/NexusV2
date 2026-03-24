namespace SoulForge.Viewer
{
    public readonly struct ViewerCommandValidationResult
    {
        public ViewerCommandValidationResult(bool isValid, string reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        public bool IsValid { get; }
        public string Reason { get; }

        public static ViewerCommandValidationResult Ok()
        {
            return new ViewerCommandValidationResult(true, "ok");
        }

        public static ViewerCommandValidationResult Fail(string reason)
        {
            return new ViewerCommandValidationResult(false, reason);
        }
    }
}
