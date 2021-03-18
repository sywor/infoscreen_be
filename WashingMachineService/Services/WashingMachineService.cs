using System;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace WashingMachineService.Services
{
    public class WashingMachineService : WashingMachineFetcher.WashingMachineFetcherBase
    {
        private readonly ILogger<WashingMachineService> logger;

        public WashingMachineService(ILogger<WashingMachineService> _logger)
        {
            logger = _logger;
        }

        public override Task<MachineStatus> GetWashingMachineStatus(Empty _request, ServerCallContext _context)
        {
            
            
            
            return Task.FromResult(new MachineStatus
            {
                State = MachineState.Offline,
                DoreState = DoorState.Closed,
                ProgramName = string.Empty,
                TimeLeft = Duration.FromTimeSpan(TimeSpan.Zero)
            });
        }
    }
}