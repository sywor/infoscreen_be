using System.Collections.Generic;
using System.Threading.Tasks;

using FrontendAPI.Data;
using FrontendAPI.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FrontendAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> logger;
        private readonly NewsGrpcService service;

        public NewsController(ILogger<NewsController> _logger, NewsGrpcService _service)
        {
            logger = _logger;
            service = _service;
        }

        [HttpGet]
        public async Task<List<NewsArticleResponse>> Get([FromQuery(Name = "articleKey")] string _articleKey)
        {
            if (string.IsNullOrEmpty(_articleKey))
            {
                return await service.GetAllArticles();
            }

            return await service.GetSingleArticle(_articleKey);
        }
    }
}