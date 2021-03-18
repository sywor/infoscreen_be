using System.Collections.Generic;
using System.Threading.Tasks;

using Common;

using FrontendAPI.Config;
using FrontendAPI.Data;
using FrontendAPI.Extensions;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using WeatherService;

namespace FrontendAPI.Services
{
    public class WeatherGrpcService
    {
        private readonly WeatherFetcher.WeatherFetcherClient client;

        public WeatherGrpcService(WeatherServiceConfiguration _config)
        {
            var grpcChannel = GrpcChannel.ForAddress(_config.Host);
            client = new WeatherFetcher.WeatherFetcherClient(grpcChannel);
        }

        public async Task<List<WeatherReportResponse>> GetAllWeatherReports()
        {
            var response = client.GetAllWeatherReports(new Empty());

            var stream = response.ResponseStream;
            var result = new List<WeatherReportResponse>();

            while (await stream.MoveNext(default))
            {
                var current = stream.Current;
                if (current.Status == null)
                {
                    result.Add(current.Weather.ToResponse());
                }
            }
            return result;
        }

        public WeatherReportResponse GetLatestWeatherReport()
        {
            var response = client.GetLatestWeatherReport(new Empty());
            if (response.Status == null)
            {
                return response.Weather.ToResponse();
            }

            return default;
        }
    }
}