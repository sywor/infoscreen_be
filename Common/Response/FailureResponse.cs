namespace Common.Response
{
    public readonly struct FailureResponse : IResponse
    {
        public static readonly FailureResponse Instance = new();

        public bool Success => false;
    }
}