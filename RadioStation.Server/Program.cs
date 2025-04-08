using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using Microsoft.Extensions.Logging;
using NLog;
using Microsoft.AspNetCore.Hosting;
using RadioStation.Server;
using RadioStation.Server.Extensions;

class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += ServiceUnhandledException;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureLogging(logging =>
               {
                   logging.ClearProviders();
               })
               .UseNLog()
               .ConfigureAppConfiguration((hostContext, configApp) =>
               {
                   configApp.AddEnvironmentVariables();
                   configApp.AddCommandLine(args);
               })
               .ConfigureServices((hostContext, services) =>
               {
                    services.AddGrpcHealthCheckOptions(nameof(RadioStationServer));
                   // services.AddHealthCheckHostedService()
                   //        .AddSqlServer(conf.BetsDBConnectionString!, name: "Bets");
                   // services.AddPrometheusDotNetMetrics(hostContext.Configuration);
               }).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void ServiceUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error("Unhandled exception occured in RadioStation.Server Service. \r\n" + e.ExceptionObject);
        }
    }