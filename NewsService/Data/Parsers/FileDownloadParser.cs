using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;

namespace NewsService.Data.Parsers
{
    public class FileDownloadParser : IResponseParser
    {
        private readonly ILogger logger;
        private readonly string bucketName;
        private readonly string filePrefix;
        private readonly string bucketDirectory;
        private readonly string staticEndpoint;
        private readonly MinioClient minioClient;

        public FileDownloadParser(IConfiguration _configuration, ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<FileDownloadParser>();

            string? minioEndpoint = null;
            string? accessKey = null;
            string? secretKey = null;

            foreach (var xpath in _configuration.GetSection("Minio").GetChildren())
            {
                switch (xpath.Key)
                {
                    case "MinioEndpoint":
                        minioEndpoint = xpath.Get<string>();
                        break;
                    case "StaticHostEndpoint":
                        staticEndpoint = xpath.Get<string>();
                        break;
                    case "AccessKey":
                        accessKey = xpath.Get<string>();
                        break;
                    case "SecretKey":
                        secretKey = xpath.Get<string>();
                        break;
                    case "BucketName":
                        bucketName = xpath.Get<string>();
                        break;
                    case "FilePrefix":
                        filePrefix = xpath.Get<string>();
                        break;
                    case "BucketDirectory":
                        bucketDirectory = xpath.Get<string>();
                        break;
                }
            }

            if (string.IsNullOrEmpty(minioEndpoint) ||
                string.IsNullOrEmpty(accessKey) ||
                string.IsNullOrEmpty(secretKey) ||
                string.IsNullOrEmpty(bucketName) ||
                string.IsNullOrEmpty(filePrefix) ||
                string.IsNullOrEmpty(bucketDirectory))
            {
                logger.LogError("Invalid configuration for minio, some value is missing or empty");
                throw new ArgumentException("Invalid configuration for minio, some value is missing or empty");
            }

            minioClient = new MinioClient(minioEndpoint, accessKey, secretKey);
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            async Task<IResponse> ParseFileDownload()
            {
                try
                {
                    var response = await _responseContent.ReadAsStreamAsync();

                    var contentType = _responseContent.Headers.ContentType;
                    var mediaType = contentType.MediaType;
                    var extension = mediaType.Split('/').Last();
                    var fullFileName = $"{bucketDirectory}/{filePrefix}_{Guid.NewGuid()}.{extension}";

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

            return await Task.Run(ParseFileDownload);
        }
    }
}