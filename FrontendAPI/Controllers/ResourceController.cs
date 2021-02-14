using System.Linq;
using System.Threading.Tasks;

using Common.Minio;

using FrontendAPI.Services;

using Microsoft.AspNetCore.Mvc;

namespace FrontendAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ResourceMinioService service;

        public ResourceController(ResourceMinioService _service)
        {
            service = _service;
        }

        [Route("{bucket}/{directory}/{filename}")]
        [HttpGet]
        public async Task<FileResult> Get([FromRoute(Name = "bucket")] string _bucket,
                                          [FromRoute(Name = "directory")] string _directory,
                                          [FromRoute(Name = "filename")] string _filename)
        {
            var extension = _filename.Split('.').Last();
            var contentType = extension == "png" ? "image/png" : "image/gif";
            return File(await service.GetResource(MinioFile.Of(_bucket, _directory, _filename)), contentType);
        }
    }
}