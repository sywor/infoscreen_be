namespace Common.Response
{
    public readonly struct SuccessResponse : IResponse
    {
        public static readonly SuccessResponse Instance = new();
        public bool Success => true;
    }
}