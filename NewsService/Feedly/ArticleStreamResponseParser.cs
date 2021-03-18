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
    public class ArticleStreamResponseParser : IResponseParser
    {
        private readonly ILogger logger;
        private const string FailedToParse = "Failed to parse feedly article stream result";

        public ArticleStreamResponseParser(ILoggerFactory _loggerFactory)
        {
            logger = _loggerFactory.CreateLogger<ArticleStreamResponseParser>();
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            try
            {
                var response = await _responseContent.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                using var jsonReader = new JsonTextReader(new StringReader(response));

                var articleStream = serializer.Deserialize<JFeedlyArticleStream>(jsonReader);

                if (articleStream == null)
                {
                    logger.LogError(FailedToParse);
                    return FailureResponse.Instance;
                }

                return new FeedlyArticleStreamResponse
                {
                    ArticleStream = articleStream
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, FailedToParse);
            }

            return FailureResponse.Instance;
        }
    }
}