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
    public class WeatherController : ControllerBase
    {
        private readonly ILogger<WeatherController> logger;
        private readonly WeatherGrpcService service;

        public WeatherController(ILogger<WeatherController> _logger, WeatherGrpcService _service)
        {
            logger = _logger;
            service = _service;
        }

        [Route("/Latest")]
        [HttpGet]
        public List<WeatherReportResponse> GetLatest()
        {
            return service.GetLatestWeatherReport();
        }

        [Route("/All")]
        [HttpGet]
        public async Task<List<WeatherReportResponse>> GetAll()
        {
            return await service.GetAllWeatherReports();
        }
    }
}