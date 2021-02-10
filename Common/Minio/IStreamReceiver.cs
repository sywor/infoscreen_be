using System.IO;

namespace Common.Minio
{
    public interface IStreamReceiver<T>
    {
        void Receive(Stream _stream);
        T Get();
    }
}