using System.Linq;

using Common;

using FrontendAPI.Data;

using NodaTime.Serialization.Protobuf;

using WeatherService;

namespace FrontendAPI.Extensions
{
    public static class Protobuf
    {
        public static string ToUrlString(this ProtoMinioFile _proto)
        {
            return $"{_proto.Bucket}/{_proto.Directory}/{_proto.FileName}";
        }

        public static PrecipitationResponse ToResponse(this PrecipitationProto _proto)
        {
            return new()
            {
                LastHour = _proto.LastHour,
                Last3Hours = _proto.Last3Hours,
                Last12Hours = _proto.Last12Hours,
                Last24Hours = _proto.Last24Hours
            };
        }

        public static WeatherForecastResponse ToResponse(this WeatherForecastProto _proto)
        {
            var forecast = _proto.Forecast.Select(_bar => new ForecastBarResponse
            {
                Precipitation = _bar.Precipitation,
                Temperature = _bar.Temperature,
                WeatherIcon = _bar.WeatherIcon.ToUrlString(),
                WindSpeed = _bar.WindSpeed,
                WindDirectionIcon = _bar.WindDirectionIcon.ToUrlString()
            }).ToList();

            return new WeatherForecastResponse
            {
                StartTime = _proto.StartTime.ToInstant(),
                EndTime = _proto.EndTime.ToInstant(),
                MaxTemp = _proto.MaxTemp,
                MinTemp = _proto.MinTemp,
                MaxWindSpeed = _proto.MaxWindSpeed,
                MinWindSpeed = _proto.MinWindSpeed,
                Forecast = forecast
            };
        }

        public static WeatherReportResponse ToResponse(this WeatherProto _report)
        {
            return new()
            {
                WeatherIcon = _report.WeatherIcon.ToUrlString(),
                WeatherDescription = _report.WeatherDescription,
                Temperature = _report.Temperature,
                WindSpeed = _report.WindSpeed,
                WindGustSpeed = _report.WindGustSpeed,
                WindDirection = _report.WindDirection.ToUrlString(),
                RadarImage = _report.RadarImage.ToUrlString(),
                PrecipitationValue = _report.PrecipitationValue.ToResponse(),
                Humidity = _report.Humidity,
                Visibility = _report.Visibility,
                CloudCover = _report.CloudCover,
                WeatherForecast = _report.WeatherForecast.ToResponse()
            };
        }
    }
}