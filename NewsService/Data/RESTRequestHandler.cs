using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewsService.Data.Parsers;

namespace NewsService.Fetchers
{
    public class RESTRequestHandler
    {
        public static async Task<IResponse> SendGetRequestAsync(string _url, IResponseParser _responseParser, ILogger _logger)
        {
            return await SendGetRequestAsync(_url, new Dictionary<string, string>(), _responseParser, _logger);
        }

        public static async Task<IResponse> SendGetRequestAsync(string _url, Dictionary<string, string> _headers, IResponseParser _responseParser, ILogger _logger)
        {
            var client = new HttpClient();

            var requestMessage =
                new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_url),
                };

            if (_headers.Any())
            {
                foreach (var (key, value) in _headers)
                {
                    requestMessage.Headers.Add(key, value);
                }
            }

            try
            {
                var responseMessage = await client.SendAsync(requestMessage);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return await _responseParser.ParseAsync(responseMessage.Content);
                }

                _logger.LogWarning("Failed to send request to: {URL}. Response code back was: {StatusCode}", _url, responseMessage.StatusCode);
            }
            catch (Exception _ex)
            {
                _logger.LogError(_ex, "Exception when sending request");
            }

            return FailureResponse.Instance;
        }

        public static async Task<IResponse> SendPostRequestAsync(string _url, Dictionary<string, string> _content, IResponseParser _responseParser, ILogger _logger)
        {
            var client = new HttpClient();

            try
            {
                var message = await client.PostAsync(_url, new FormUrlEncodedContent(_content));

                if (message.StatusCode == HttpStatusCode.OK)
                {
                    return await _responseParser.ParseAsync(message.Content);
                }

                _logger.LogWarning("Failed to send request to: {URL}. Response code back was: {StatusCode}", _url, message.StatusCode);
            }
            catch (Exception _ex)
            {
                _logger.LogError(_ex, "Exception when sending request");
            }

            return FailureResponse.Instance;
        }
    }
}