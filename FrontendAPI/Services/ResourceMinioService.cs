using System.IO;
using System.Threading.Tasks;

using Common.Config;
using Common.Minio;

using Microsoft.AspNetCore.Mvc;

namespace FrontendAPI.Services
{
    public class ResourceMinioService
    {
        private readonly MinioService minioService;

        public ResourceMinioService(MinioConfiguration _configuration)
        {
            minioService = new MinioService(_configuration);
        }

        public async Task<Stream> GetResource(MinioFile _minioFile)
        {
            return _minioFile.Bucket switch
            {
                "ephemeral" => await minioService.GetEphemeralObjectAsync<Stream, StreamReceiver>(_minioFile),
                "static"    => await minioService.GetStaticObjectAsync<Stream, StreamReceiver>(_minioFile),
                _           => null
            };
        }
    }
}