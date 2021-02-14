using Common;
using Common.Minio;

namespace FrontendAPI.Extensions
{
    public static class Protobuf
    {
        public static MinioFile ToMinio(this ProtoMinioFile _file)
        {
            return new MinioFile
            {
                Bucket = _file.Bucket,
                Directory = _file.Directory,
                FileName = _file.FileName
            };
        }
    }
}