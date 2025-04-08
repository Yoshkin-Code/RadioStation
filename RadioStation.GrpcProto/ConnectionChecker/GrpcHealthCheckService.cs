using Grpc.Core;
using Grpc.Health.V1;
using NLog;

namespace RadioStation.GrpcProto.ConnectionChecker;

public class GrpcHealthCheckService(GrpcHealthCheckOptions options) : Grpc.Health.V1.Health.HealthBase
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    public override Task<HealthCheckResponse> Check(
        HealthCheckRequest request,
        ServerCallContext context)
    {
        if (!string.IsNullOrEmpty(options.Service) && !string.IsNullOrEmpty(request.Service) &&
            request.Service != options.Service)
            return Task.FromResult(this.SendUnknown());
        try
        {
            return Task.FromResult(this.SendOk());
        }
        catch (Exception ex)
        {
            _log.Warn(ex.Message);
            HealthCheckTerminationLog.Dump(ex.Message);
            return Task.FromResult(SendError());
        }
    }

    private HealthCheckResponse SendOk()
    {
        return new HealthCheckResponse()
        {
            Status = HealthCheckResponse.Types.ServingStatus.Serving
        };
    }

    private HealthCheckResponse SendError()
    {
        return new HealthCheckResponse()
        {
            Status = HealthCheckResponse.Types.ServingStatus.NotServing
        };
    }

    private HealthCheckResponse SendUnknown()
    {
        return new HealthCheckResponse()
        {
            Status = HealthCheckResponse.Types.ServingStatus.Unknown
        };
    }
}