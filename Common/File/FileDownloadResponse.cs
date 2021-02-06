using Common.Response;

namespace Common.File
{
    public readonly struct FileDownloadResponse : IResponse
    {
        public bool Success => true;
        public string FileUri { get; init; }
    }
}