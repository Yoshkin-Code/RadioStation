using RadioStation.GrpcProto;
using RadioStation.Server.Service;

namespace RadioStation.Server.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddGrpcHealthCheckOptions(
        this IServiceCollection services,
        string serviceName)
    {
        if (services == null)
            throw new ArgumentNullException(nameof (services));
        services.AddSingleton<GrpcHealthCheckOptions>(new GrpcHealthCheckOptions()
        {
            Service = serviceName
        });
        return services;
    }
    
}