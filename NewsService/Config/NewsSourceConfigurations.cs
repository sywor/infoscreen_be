using System.Collections.Generic;
using System.Linq;

namespace NewsService.Config
{
    public class NewsSourceConfigurations
    {
        private readonly Dictionary<string, NewsSourceConfiguration> configurations;

        public NewsSourceConfigurations(IEnumerable<NewsSourceConfiguration> _configurations)
        {
            configurations = _configurations.ToDictionary(_cfg => _cfg.Name);
        }

        public NewsSourceConfiguration this[string _index] => configurations[_index];
    }
}