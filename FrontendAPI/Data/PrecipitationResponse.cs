namespace FrontendAPI.Data
{
    public readonly struct PrecipitationResponse
    {
        public float LastHour { get; init; }
        public float Last3Hours { get; init; }
        public float Last12Hours { get; init; }
        public float Last24Hours { get; init; }
    }
}