using System.Collections.Generic;

using Common.Minio;

namespace WeatherService.Data
{
    public class ResourceRegister
    {
        private record WeatherSymbol(MinioFile MinioFile, string Description);

        private const string Directory = "icons";
        private const string Bucket = "static";

        private static readonly Dictionary<int, WeatherSymbol> IntToSymbolDict = new()
        {
            {1, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "ClearSky.png"), "Clear skies")},
            {2, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "NearlyClearSky.png"), "Nearly clear skies")},
            {3, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "VariableCloudiness.png"), "Variable cloudiness")},
            {4, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "HalfClearSky.png"), "Half clear skies")},
            {5, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "CloudySky.png"), "Cloudy skies")},
            {6, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "Overcast.png"), "Overcast")},
            {7, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "Fog.png"), "Fog")},
            {8, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "LightRain.png"), "Light rain")},
            {9, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "ModerateRain.png"), "Moderate rain")},
            {10, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "HeavyRain.png"), "Heavy rain")},
            {11, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "Thunderstorm.png"), "Thunderstorm")},
            {12, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Light sleet showers")},
            {13, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Moderate sleet showers")},
            {14, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Heavy sleet showers")},
            {15, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "LightSnow.png"), "Light snow showers")},
            {16, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "ModerateSnow.png"), "Moderate snow showers")},
            {17, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "HeavySnow.png"), "Heavy snow showers")},
            {18, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "LightRain.png"), "Light rain")},
            {19, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "ModerateRain.png"), "Moderate rain")},
            {20, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "HeavyRain.png"), "Heavy rain")},
            {21, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "Thunder.png"), "Thunder")},
            {22, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Light sleet")},
            {23, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Moderate sleet")},
            {24, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "SleetAndFrozenRain.png"), "Heavy sleet")},
            {25, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "LightSnow.png"), "Light snowfall")},
            {26, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "ModerateSnow.png"), "Moderate snowfall")},
            {27, new WeatherSymbol(MinioFile.Of(Bucket,Directory, "HeavySnow.png"), "Heavy snowfall")}
        };

        private static readonly MinioFile North = MinioFile.Of(Bucket, Directory, "North.png");
        private static readonly MinioFile NorthEast = MinioFile.Of(Bucket, Directory, "NorthEast.png");
        private static readonly MinioFile East = MinioFile.Of(Bucket, Directory, "East.png");
        private static readonly MinioFile SouthEast = MinioFile.Of(Bucket, Directory, "SouthEast.png");
        private static readonly MinioFile South = MinioFile.Of(Bucket, Directory, "South.png");
        private static readonly MinioFile SouthWest = MinioFile.Of(Bucket, Directory, "SouthWest.png");
        private static readonly MinioFile West = MinioFile.Of(Bucket, Directory, "West.png");
        private static readonly MinioFile NorthWest = MinioFile.Of(Bucket, Directory, "NorthWest.png");

        public static MinioFile LookupWeatherSymbol(int _value)
        {
            return IntToSymbolDict[_value].MinioFile;
        }

        public static MinioFile LookupWindDirection(int _value)
        {
            if ((_value >= 337 && _value <= 360) || (_value >= 0 && _value < 22))
            {
                return South;
            }
            if (_value >= 22 && _value < 67)
            {
                return SouthWest;
            }
            if (_value >= 67 && _value < 112)
            {
                return West;
            }
            if (_value >= 112 && _value < 157)
            {
                return NorthWest;
            }
            if (_value >= 157 && _value < 202)
            {
                return North;
            }
            if (_value >= 202 && _value < 247)
            {
                return NorthEast;
            }
            if (_value >= 247 && _value < 292)
            {
                return East;
            }

            return SouthEast;
        }

        public static string LookupWeatherDescription(int _value)
        {
            return IntToSymbolDict[_value].Description;
        }
    }
}