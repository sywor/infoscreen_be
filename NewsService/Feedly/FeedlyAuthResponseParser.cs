using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Common.Response;

using Microsoft.Extensions.Logging;

using NewsService.Data;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace NewsService.Feedly
{
    public class FeedlyAuthResponseParser : IResponseParser
    {
        private readonly ILogger logger;
        private const string FailedToParse = "Failed to parse feedly auth result";

        public FeedlyAuthResponseParser(ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<FeedlyAuthResponseParser>();
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            try
            {
                var response = await _responseContent.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                using var jsonReader = new JsonTextReader(new StringReader(response));

                var tokens = serializer.Deserialize<FeedlyAuthResponse>(jsonReader);

                if (tokens != null)
                {
                    return tokens;
                }

                logger.LogError(FailedToParse);
            }
            catch (Exception e)
            {
                logger.LogError(e, FailedToParse);
            }

            return FailureResponse.Instance;
        }
    }
}