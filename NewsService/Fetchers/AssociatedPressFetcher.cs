using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace NewsService.Fetchers
{
    public class AssociatedPressFetcher : AbstractWebPageFetcher<AssociatedPressFetcher>
    {
        private const string NAME = "associated_press";

        public AssociatedPressFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Published at could be found for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            var srcValue = _node.First().GetAttributeValue("data-source", null);

            if (srcValue == null)
            {
                Logger.LogWarning($"Published at tag was empty for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            _value = PublishedAtPattern
                .Parse(srcValue)
                .Value.InUtc();

            return true;
        }
    }
}