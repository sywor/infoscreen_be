using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace WashingMachineService.Services
{
    public class WashingMashineService : WashingMachineFetcher.WashingMachineFetcherBase
    {
        private readonly ILogger<WashingMashineService> logger;

        public WashingMashineService(ILogger<WashingMashineService> _logger)
        {
            logger = _logger;
        }

        public override Task<MachineStatus> GetWashingMachineStatus(Empty _request, ServerCallContext _context)
        {
            return base.GetWashingMachineStatus(_request, _context);
        }
    }
}