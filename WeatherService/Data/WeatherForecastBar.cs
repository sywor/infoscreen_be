namespace WeatherService.Data
{
    public readonly struct WeatherForecastBar
    {
        public float Temprature { get; init; }
        public float PrecipitationValue { get; init; }
        public float WindSpeed { get; init; }
        public int WindDirection { get; init; }
        public int WeatherSymbol { get; init; }
    }
}