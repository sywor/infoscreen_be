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
    public class CurrentWeatherParser : WeatherParser, IResponseParser
    {
        private readonly ILogger logger;

        public CurrentWeatherParser(ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<CurrentWeatherParser>();
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            async Task<IResponse> ParseWeather()
            {
                try
                {
                    var response = await _responseContent.ReadAsStringAsync();

                    var serializer = new JsonSerializer();
                    serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                    using var jsonReader = new JsonTextReader(new StringReader(response));

                    var jCurrentWeather = serializer.Deserialize<JWeather>(jsonReader);

                    if (jCurrentWeather == null)
                    {
                        logger.LogError("No current weather received");
                        return FailureResponse.Instance;
                    }

                    var jTimeSeries = jCurrentWeather.TimeSeries;
                    var precipitation = jTimeSeries.Select(series => series.Parameters)
                                                   .Select(MapParameterName)
                                                   .Select(dict => dict["prec1h"].Values.First())
                                                   .ToList();

                    var paramNameDict = MapParameterName(jCurrentWeather.TimeSeries.First().Parameters);

                    var result = new CurrentWeather()
                    {
                        ReferenceTime = jCurrentWeather.ReferenceTime,
                        Temperature = paramNameDict["t"].Values.First(),
                        WindSpeed = paramNameDict["ws"].Values.First(),
                        WindGustSpeed = paramNameDict["gust"].Values.First(),
                        WindDirectionIcon = (int) paramNameDict["wd"].Values.First(),
                        Humidity = (int) paramNameDict["r"].Values.First(),
                        Visibility = paramNameDict["vis"].Values.First(),
                        CloudCover = (int) paramNameDict["tcc"].Values.First(),
                        WeatherIcon = (int) paramNameDict["Wsymb2"].Values.First(),
                        Precipitation = precipitation
                    };
                    
                    
                    logger.LogInformation("Parsed current weather");

                    return new CurrentWeatherResponse
                    {
                        CurrentWeather = result
                    };
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to parse current weather response");
                }

                return FailureResponse.Instance;
            }

            return await Task.Run(ParseWeather);
        }
    }
}