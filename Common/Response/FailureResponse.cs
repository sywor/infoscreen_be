namespace Common.Response
{
    public class FailureResponse : IResponse
    {
        public static readonly FailureResponse Instance = new FailureResponse();

        public bool Success => false;
    }
}