using System.IO;
using System.Threading.Tasks;

using Common.Config;

using Minio;

namespace Common.Minio
{
    public class MinioService
    {
        private readonly string ephemeralBucketName;
        private readonly string staticBucketName;
        private readonly string bucketDirectory;
        private readonly MinioClient minioClient;

        public MinioService(MinioConfiguration _configuration)
        {
            ephemeralBucketName = _configuration.EphemeralBucketName!;
            staticBucketName = _configuration.StaticBucketName!;
            bucketDirectory = _configuration.BucketDirectory!;

            minioClient = new MinioClient(_configuration.MinioEndpoint, _configuration.AccessKey, _configuration.SecretKey);
        }

        public async Task PutEphemeralObjectAsync(MinioFile _minioFile, Stream _stream, string? _contentType = null)
        {
            if (!await minioClient.BucketExistsAsync(ephemeralBucketName))
            {
                await minioClient.MakeBucketAsync(ephemeralBucketName);
            }

            var fullFileName = $"{bucketDirectory}/{_minioFile.Directory}/{_minioFile.FileName}";

            _stream.Position = 0;
            await minioClient.PutObjectAsync(ephemeralBucketName, fullFileName, _stream, _stream.Length, _contentType);
        }

        public async Task<T> GetStaticObjectAsync<T, S>(MinioFile _minioFile) where S : IStreamReceiver<T>, new()
        {
            return await GetObjectAsync<T, S>(staticBucketName, _minioFile);
        }

        public async Task<T> GetEphemeralObjectAsync<T, S>(MinioFile _minioFile) where S : IStreamReceiver<T>, new()
        {
            return await GetObjectAsync<T, S>(ephemeralBucketName, _minioFile);
        }

        private async Task<T> GetObjectAsync<T, S>(string _bucketName, MinioFile _minioFile) where S : IStreamReceiver<T>, new()
        {
            var fullFileName = $"{bucketDirectory}/{_minioFile.Directory}/{_minioFile.FileName}";

            var receiver = new S();
            await minioClient.GetObjectAsync(_bucketName, fullFileName, _stream => receiver.Receive(_stream));

            return receiver.Get();
        }
    }
}