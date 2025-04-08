using RadioStation.GrpcProto.ConnectionChecker;
using RadioStation.Server.API;
using RadioStation.Server.Extensions;
using RadioStation.Server.Service;

namespace RadioStation.Server;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();
        
        services.AddGrpcHealthCheckOptions("RadioStation");
        services.AddHostedService<RadioStationServerHostedService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<GrpcHealthCheckService>();
            endpoints.MapGrpcService<ServiceApiHost>();

            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client");
            });
        });
    }
}