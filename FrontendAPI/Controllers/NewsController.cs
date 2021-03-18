using System.Collections.Generic;
using System.Threading.Tasks;

using FrontendAPI.Data;
using FrontendAPI.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FrontendAPI.Controllers
{
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> logger;
        private readonly NewsGrpcService service;

        public NewsController(ILogger<NewsController> _logger, NewsGrpcService _service)
        {
            logger = _logger;
            service = _service;
        }

        [Route("/News/All")]
        [HttpGet]
        public async Task<List<NewsArticleResponse>> GetAll()
        {
            return await service.GetAllArticles();
        }
        
        [Route("/News/Single")]
        [HttpGet]
        public async Task<NewsArticleResponse> GetSingle([FromQuery(Name = "articleKey")] string _articleKey)
        {
            return await service.GetSingleArticle(_articleKey);
        }
        
        [Route("/News/Next")]
        [HttpGet]
        public async Task<NewsArticleResponse> GetNext()
        {
            return await service.GetNextArticle();
        }
    }
}