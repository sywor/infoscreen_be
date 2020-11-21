namespace NewsService.Data.Parsers
{
    public class FileDownloadResponse : IResponse
    {
        public bool Success => true;
        public string FileUri { get; set; }
    }
}