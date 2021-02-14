using System.IO;

using Common.Minio;

namespace FrontendAPI.Services
{
    public class StreamReceiver : IStreamReceiver<Stream>
    {
        private MemoryStream stream;

        public void Receive(Stream _stream)
        {
            stream = new MemoryStream();
            _stream.CopyTo(stream);
        }

        public Stream Get()
        {
            stream.Position = 0;
            return stream;
        }
    }
}