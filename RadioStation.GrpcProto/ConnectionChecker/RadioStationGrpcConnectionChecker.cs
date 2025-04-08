using Grpc.Core;
using Microsoft.Extensions.Logging;
using RadioStationServer.GrpcProto;

namespace RadioStation.GrpcProto.ConnectionChecker
{
    public class RadioStationGrpcConnectionChecker(
        RadioControl.RadioControlClient radioControlClient,
        ILogger<RadioStationGrpcConnectionChecker> logger)
        : IGrpcConnectionChecker<RadioControl.RadioControlClient>
    {
        private const byte MAX_RETRIES_COUNT = 2;
        private byte _retryCounter = 0;

        /// <summary>
        /// Pings service API asynchronously.
        /// If the <see cref="IsRPCConnected()"/> were not connected
        /// it would do retries for <see cref="MAX_RETRIES_COUNT"/> times
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsRPCConnectedAsync(CancellationToken token, int? deadLine)
        {
            try
            {
                DateTime? deadlineDate = deadLine.HasValue ? DateTime.UtcNow.AddSeconds(deadLine.GetValueOrDefault()) : (DateTime?)null;
                await radioControlClient.PingAsync(new PingRequest(), cancellationToken: token, deadline: deadlineDate);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is RpcException && _retryCounter < MAX_RETRIES_COUNT)
                {
                    _retryCounter++;
                    logger.LogError($"Grpc service is unreachable. Retry {_retryCounter}, of all {MAX_RETRIES_COUNT}, try again. {ex}");
                    await IsRPCConnectedAsync(token, deadLine);
                }
                else
                {
                    throw new Exception($"Internal Error / Grpc service is unreachable, Retry {_retryCounter}, of all {MAX_RETRIES_COUNT}. {ex}");
                }
                return false;
            }
        }
    }
}
