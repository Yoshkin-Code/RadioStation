using Grpc.Core;

namespace RadioStation.GrpcProto.ConnectionChecker
{
    public interface IGrpcConnectionChecker<in T> where T : ClientBase<T>
    {
        /// <summary>
        /// Pings service API asynchronously.
        /// </summary>
        /// <param name="token">Cancel operation</param>
        /// <param name="deadLine">Describes the deadline value in seconds
        /// of a connection loss to proceed the reconnection
        /// attempt</param>
        /// <returns></returns>
        Task<bool> IsRPCConnectedAsync(CancellationToken token, int? deadLine);
    }
}
