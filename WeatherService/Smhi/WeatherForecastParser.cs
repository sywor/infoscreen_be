using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Common.Response;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Serialization.JsonNet;

using WeatherService.Data;
using WeatherService.Json;

namespace WeatherService.Smhi
{
    public class WeatherForecastParser : WeatherParser, IResponseParser
    {
        private readonly ILogger logger;

        public WeatherForecastParser(ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<WeatherForecastParser>();
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            async Task<IResponse> ParseWeatherForecast()
            {
                try
                {

                    var response = await _responseContent.ReadAsStringAsync();

                    var serializer = new JsonSerializer();
                    serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                    using var jsonReader = new JsonTextReader(new StringReader(response));

                    var jForecast = serializer.Deserialize<JWeather>(jsonReader);

                    if (jForecast == null)
                    {
                        logger.LogError("No forecast received");
                        return FailureResponse.Instance;
                    }

                    var weatherForecastBars = jForecast.TimeSeries.Select(_series => _series.Parameters)
                                                       .Select(MapParameterName)
                                                       .Select(_paramNameDict => new WeatherForecastBar
                                                       {
                                                           Temprature = _paramNameDict["t"].Values.First(),
                                                           PrecipitationValue = _paramNameDict["pmean"].Values.First(),
                                                           WindSpeed = _paramNameDict["ws"].Values.First(),
                                                           WindDirection = (int)_paramNameDict["wd"].Values.First(),
                                                           WeatherSymbol = (int)_paramNameDict["Wsymb2"].Values.First()
                                                       })
                                                       .ToList();

                    var result = new WeatherForecast
                    {
                        StartDate = jForecast.TimeSeries.First().ValidTime,
                        EndDate = jForecast.TimeSeries.Last().ValidTime,
                        WeatherForecastBars = weatherForecastBars,
                        MaxTemp = weatherForecastBars.Max(_bar => _bar.Temprature),
                        MinTemp = weatherForecastBars.Min(_bar => _bar.Temprature),
                        MaxWindSpeed = weatherForecastBars.Max(_bar => _bar.WindSpeed),
                        MinWindSpeed = weatherForecastBars.Min(_bar => _bar.WindSpeed)
                    };

                    logger.LogInformation("Parsed weather forecast");

                    return new WeatherForecastResponse
                    {
                        WeatherForecast = result
                    };
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to parse weather forecast response");
                }

                return FailureResponse.Instance;
            }

            return await Task.Run(ParseWeatherForecast);
        }
    }
}