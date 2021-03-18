using System.Collections.Generic;
using System.Threading.Tasks;

using Common;
using Common.Recurrence;
using Common.Redis;

using Microsoft.Extensions.Logging;

using NodaTime;

using WashingMachineService.Data;

namespace WashingMachineService.WashingMachine
{
    public class HomeConnectFetcher : IRunnable
    {
        private readonly IRedisCacheService redis;
        private readonly ILogger<HomeConnectFetcher> logger;
        private const string RedisKey = "washing_machine_state";
        private const string ServerUrlTemplate = "{{serverUrl}}";
        private const string HaidTemplate = "{{haid}}";
        private const string RefreshTokenUrl = "{{serverUrl}}/security/oauth/token";
        private const string ActiveProgramUrl = "{{serverUrl}}/api/homeappliances/{{haid}}/programs/active";
        private const string MachineStatusUrl = "{{serverUrl}}/api/homeappliances/{{haid}}/status";
        private readonly RefreshTokenParser refreshTokenParser = new();

        public HomeConnectFetcher(ILoggerFactory _loggerFactory, IRedisCacheService _redis)
        {
            redis = _redis;
            logger = _loggerFactory.CreateLogger<HomeConnectFetcher>();
        }

        public async Task Run()
        {
            var response = await redis.GetValue<WashingMachineState>(RedisKey);
            
            if (!response.Success)
            {
                logger.LogError("Could not find washing machine state with key: {Key}", RedisKey);
                return;
            }
            
            var now = SystemClock.Instance.GetCurrentInstant();
            var state = ((RedisResponse<WashingMachineState>)response).Value;

            if (state.Expires < now - Duration.FromHours(1))
            {
                var replace = RefreshTokenUrl.Replace(ServerUrlTemplate, state.HomeConnectUrl);
                var headers = new Dictionary<string, string>();
                var refreshTokenResponse = await RestRequestHandler.SendGetRequestAsync(replace, headers, refreshTokenParser, logger);
            }
            
            
            
            


            var newState = new WashingMachineState
            {
                Haid = "BOSCH-WAWH26I9SN-68A40E3034C1",
                DeviceCode = "eca773e6-21f8-4562-a453-5e2b114f933b",
                ClientId = "AAFE94F04922E5F93774DC6496B250064E7F00B25294F6B6BFC7D4D34AD897DD",
                AccessToken = "eyJ4LWVudiI6IlBSRCIsImFsZyI6IlJTMjU2IiwieC1yZWciOiJFVSIsImtpZCI6IlMxIn0.eyJmZ3JwIjpbXSwiY2x0eSI6InByaXZhdGUiLCJzdWIiOiJmNGI0NDFjNy1kOTk0LTQwYzktYjI2Zi00YmFlMmQzNmIyMzciLCJhdWQiOiJBQUZFOTRGMDQ5MjJFNUY5Mzc3NERDNjQ5NkIyNTAwNjRFN0YwMEIyNTI5NEY2QjZCRkM3RDREMzRBRDg5N0REIiwiYXpwIjoiQUFGRTk0RjA0OTIyRTVGOTM3NzREQzY0OTZCMjUwMDY0RTdGMDBCMjUyOTRGNkI2QkZDN0Q0RDM0QUQ4OTdERCIsInNjb3BlIjpbIklkZW50aWZ5QXBwbGlhbmNlIiwiTW9uaXRvciJdLCJpc3MiOiJFVTpQUkQ6NDMiLCJleHAiOjE2MTM0OTIyNzQsImlhdCI6MTYxMzQwNTg3NCwianRpIjoiYzJiZDk3ZmQtOGI5OS00NmQzLWI3ZDgtNTY3OGJkZDM5NDIwIn0.KVfM6Oa2LOLSw0ul5g1GqEE1N8FUomvnTp_ErFT4UwC9lgGzGsrjszN0yAWvU5JAaOAjSPSkCatloCJ7dMPYoyhbKjhnCiLlQkxuZICcTjN9kiKrSbBNFEsIRJb9i2xYjaHNCDLeukMlZMywml0XCjUodCQNPrEBLVbqKE7GOHBe0hMLSdMUN-GNRxBjZt5o50OIpIetwhps5gNzbal3dVgVdPRnvM0N4dJou0dRel3CxUjoCXYUCdTm7j8HtG3EkUt9mMCdJ3fsypbd-G_76V-JeXJkiLQT2M_RQni7Tr71lwqOdlYJGNIK4PpDKffPcD4BjTo3EGpceMnaCf_J8Q",
                RefreshToken = "eyJ4LXJlZyI6IkVVIiwieC1lbnYiOiJQUkQiLCJ0b2tlbiI6IjkwYmZjNTZjLWU5YjgtNDdiZi1iODFlLTMzNjljYzI3YjhhNyIsImNsdHkiOiJwcml2YXRlIn0=",
                IdToken = "eyJ4LWVudiI6IlBSRCIsImFsZyI6IlJTMjU2IiwieC1yZWciOiJFVSIsImtpZCI6IlMxIn0.eyJmZ3JwIjpbXSwiY2x0eSI6InByaXZhdGUiLCJzdWIiOiJmNGI0NDFjNy1kOTk0LTQwYzktYjI2Zi00YmFlMmQzNmIyMzciLCJhdWQiOiJBQUZFOTRGMDQ5MjJFNUY5Mzc3NERDNjQ5NkIyNTAwNjRFN0YwMEIyNTI5NEY2QjZCRkM3RDREMzRBRDg5N0REIiwiYXpwIjoiQUFGRTk0RjA0OTIyRTVGOTM3NzREQzY0OTZCMjUwMDY0RTdGMDBCMjUyOTRGNkI2QkZDN0Q0RDM0QUQ4OTdERCIsInNjb3BlIjpbIklkZW50aWZ5QXBwbGlhbmNlIiwiTW9uaXRvciJdLCJpc3MiOiJFVTpQUkQ6NDMiLCJleHAiOjE2MTM0OTIyNzQsImlhdCI6MTYxMzQwNTg3NCwianRpIjoiYzJiZDk3ZmQtOGI5OS00NmQzLWI3ZDgtNTY3OGJkZDM5NDIwIn0.KVfM6Oa2LOLSw0ul5g1GqEE1N8FUomvnTp_ErFT4UwC9lgGzGsrjszN0yAWvU5JAaOAjSPSkCatloCJ7dMPYoyhbKjhnCiLlQkxuZICcTjN9kiKrSbBNFEsIRJb9i2xYjaHNCDLeukMlZMywml0XCjUodCQNPrEBLVbqKE7GOHBe0hMLSdMUN-GNRxBjZt5o50OIpIetwhps5gNzbal3dVgVdPRnvM0N4dJou0dRel3CxUjoCXYUCdTm7j8HtG3EkUt9mMCdJ3fsypbd-G_76V-JeXJkiLQT2M_RQni7Tr71lwqOdlYJGNIK4PpDKffPcD4BjTo3EGpceMnaCf_J8Q",
                Expires = now + Duration.FromSeconds(86400),
                HomeConnectUrl = "https://api.home-connect.com"
            };

            await redis.AddValue(RedisKey, newState);
        }
    }
}