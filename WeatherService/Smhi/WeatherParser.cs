using System.Collections.Generic;

using WeatherService.Json;

namespace WeatherService.Smhi
{
    public abstract class WeatherParser
    {
        protected static Dictionary<string, JParameter> MapParameterName(List<JParameter> _parameters)
        {
            var paramNameDict = new Dictionary<string, JParameter>();
            foreach (var param in _parameters)
            {
                var name = param.Name;
                paramNameDict.TryAdd(name, param);
            }

            return paramNameDict;
        }
    }
}