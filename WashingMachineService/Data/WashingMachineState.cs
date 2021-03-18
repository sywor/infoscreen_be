using NodaTime;

namespace WashingMachineService.Data
{
    public readonly struct WashingMachineState
    {
        public string Haid { get; init; }
        public string HomeConnectUrl { get; init; }
        public string AccessToken { get; init; }
        public string IdToken { get; init; }
        public string RefreshToken { get; init; }
        public Instant Expires { get; init; }
        public string ClientId { get; init; }
        public string DeviceCode { get; init; }
    }
}