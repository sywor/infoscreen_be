using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

namespace WeatherService.Services
{
    public class WeatherService : WeatherFetcher.WeatherFetcherBase
    {
        public override Task<WeatherResponseProto> GetWeatherReport(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new WeatherResponseProto());
        }
    }
}