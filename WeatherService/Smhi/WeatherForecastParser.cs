using System;
using System.Collections.Generic;
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
                    var temperatures = new List<float>();
                    var precipitationValues = new List<float>();
                    var windSpeeds = new List<float>();
                    var windDirection = new List<int>();
                    var weatherSymbols = new List<int>();

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

                    foreach (var paramNameDict in jForecast.TimeSeries.Select(series => series.Parameters).Select(MapParameterName))
                    {
                        temperatures.Add(paramNameDict["t"].Values.First());
                        precipitationValues.Add(paramNameDict["pmean"].Values.First());
                        windSpeeds.Add(paramNameDict["ws"].Values.First());
                        windDirection.Add((int) paramNameDict["wd"].Values.First());
                        weatherSymbols.Add((int) paramNameDict["Wsymb2"].Values.First());
                    }

                    var result = new WeatherForecast
                    {
                        LeftDate = jForecast.TimeSeries.First().ValidTime,
                        RightDate = jForecast.TimeSeries.Last().ValidTime,
                        Temperatures = temperatures,
                        PrecipitationValues = precipitationValues,
                        WindSpeeds = windSpeeds,
                        WindDirection = windDirection,
                        WeatherSymbols = weatherSymbols
                    };

                    logger.LogInformation("Parsed weather forecast: {Forecast}", result);

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