namespace NewsService.Data.Parsers
{
    public class FailureResponse : IResponse
    {
        public static readonly FailureResponse Instance = new FailureResponse();

        public bool Success => false;
    }
}