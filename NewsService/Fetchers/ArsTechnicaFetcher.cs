using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class ArsTechnicaFetcher : AbstractRssFetcher<ArsTechnicaFetcher>
    {
        private const string NAME = "arstechnica";
        private readonly Regex regex = new Regex(@".*'(http.+)'.*", RegexOptions.Compiled);

        public ArsTechnicaFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (base.ExtractImage(_node, _url, out _value))
            {
                return true;
            }

            Logger.LogWarning("Attempting fallback method");
            var srcValue = _node?.First().GetAttributeValue("style", null);

            if (srcValue == null)
            {
                Logger.LogWarning($"Image style tag couldn't be found for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            Match match = regex.Match(srcValue);

            if (!match.Success)
            {
                Logger.LogWarning($"Image style tag didn't match pattern for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            _value = match.Groups[1].Value;
            return true;
        }

        protected override bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body could be found for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            var result = _node.FirstOrDefault(_x => !string.IsNullOrEmpty(_x.InnerText))?.InnerText;

            if (result == null)
            {
                Logger.LogWarning($"Body was empty for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            _value = result;
            return true;
        }
    }
}