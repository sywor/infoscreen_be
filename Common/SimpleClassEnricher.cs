using System.Collections.Generic;
using System.Linq;

using Serilog.Core;
using Serilog.Events;

namespace Common
{
    public class SimpleClassEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent _logEvent, ILogEventPropertyFactory _propertyFactory)
        {
            var typeName = _logEvent.Properties.GetValueOrDefault("SourceContext")?.ToString();

            if(typeName == null)
                return;

            typeName = typeName.Trim('"');

            var parts = typeName.Split('.');
            var path = string.Empty;

            for (var i = 0; i < parts.Length - 1; i++)
            {
                path += (parts[i][0] + ".").ToLowerInvariant();
            }

            path += parts.Last();
            path.TrimStart('.');

            _logEvent.AddOrUpdateProperty(_propertyFactory.CreateProperty("SourceContext", path));
        }
    }
}