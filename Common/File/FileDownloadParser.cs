using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Common.Config;
using Common.Minio;
using Common.Response;

using Microsoft.Extensions.Logging;

namespace Common.File
{
    public class FileDownloadParser : IResponseParser
    {
        private readonly ILogger logger;
        private readonly MinioService minioService;
        private const string Directory = "downloaded";

        public FileDownloadParser(MinioConfiguration _configuration, ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<FileDownloadParser>();
            minioService = new MinioService(_configuration);
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

                var fullFileName = $"{prefix}_{Guid.NewGuid()}.{extension}";

                var minioFile = MinioFile.Of(Directory, fullFileName);
                await minioService.PutEphemeralObjectAsync(minioFile, response, mediaType);

                return new FileDownloadResponse
                {
                    FileLocation = minioFile
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