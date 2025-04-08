using Microsoft.Extensions.Hosting;
using NLog;

namespace RadioStation.Server.Service;

public class RadioStationServerHostedService(IHostApplicationLifetime appLifetime) : IHostedService
{
    protected readonly CancellationTokenSource _ctSource = new();
    protected readonly IHostApplicationLifetime _appLifetime = appLifetime;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    protected bool _isStopping;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Info($"Starting {nameof(RadioStationServerHostedService)} ...");
        CloseAudioService();
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);
        _logger.Info($"App lifetime registration finished...");
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        try
        {
            _logger.Info("Service started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
    }

    private void OnStopping()
    {
        try
        {
            _logger.Info("Service stopping...");
            _isStopping = true;
            _logger.Info("Canceling token...");
            _ctSource.Cancel();
            _logger.Info("Stopping broker...");
            CloseAudioService();
        }
        catch (Exception ex)
        {
            if (ex is AggregateException && ex.InnerException is OperationCanceledException)
            {
                _logger.Warn(ex, "Main task stopped.");
            }
            else
            {
                _logger.Error(ex, "Error on stopping task.");
            }
        }

        _ctSource?.Dispose();
    }

    private void OnStopped()
    {
        _logger.Info("Service stopped");
    }

    private void CloseAudioService()
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(CloseAudioService)} error");
        }
    }


    private void StopApp()
    {
        if (!_isStopping)
        {
            _logger.Info("Stopping application...");
            _appLifetime.StopApplication();
            _logger.Info("Application stopped");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Info("Service StopAsync...");
        return Task.CompletedTask;
    }
}