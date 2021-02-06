using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Common.Config;
using Common.Response;

using Microsoft.Extensions.Logging;

using Minio;

namespace Common.File
{
    public class FileDownloadParser : IResponseParser
    {
        private readonly ILogger logger;
        private readonly string bucketName;
        private readonly string bucketDirectory;
        private readonly string staticEndpoint;
        private readonly MinioClient minioClient;

        public FileDownloadParser(MinioConfiguration _configuration, ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<FileDownloadParser>();

            bucketName = _configuration.BucketName!;
            bucketDirectory = _configuration.BucketDirectory!;
            staticEndpoint = _configuration.StaticHostEndpoint!;

            minioClient = new MinioClient(_configuration.MinioEndpoint, _configuration.AccessKey, _configuration.SecretKey);
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            try
            {
                var response = await _responseContent.ReadAsStreamAsync();

                var contentType = _responseContent.Headers.ContentType;
                var mediaType = contentType?.MediaType;
                var prefix = mediaType?.Split('/').First();
                var extension = mediaType?.Split('/').Last();
                var fullFileName = $"{bucketDirectory}/{prefix}_{Guid.NewGuid()}.{extension}";

                if (!await minioClient.BucketExistsAsync(bucketName))
                {
                    await minioClient.MakeBucketAsync(bucketName);
                }

                await minioClient.PutObjectAsync(bucketName, fullFileName, response, response.Length, mediaType);

                return new FileDownloadResponse
                {
                    FileUri = $"http://{staticEndpoint}/{fullFileName}"
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to parse file download result");
            }

            return FailureResponse.Instance;
        }
    }
}