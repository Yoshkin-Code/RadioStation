using System.Collections.Concurrent;
using Grpc.Core;
using NAudio.Wave;
using NLog;
using RadioStation.Server.Service;
using RadioStationServer.GrpcProto;

namespace RadioStation.Server.API;

public class ServiceApiHost(IWebHostEnvironment env) : RadioControl.RadioControlBase
{
    public string AudioFolder => Path.Combine(env.WebRootPath, "audio");
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<AudioChunk>> _clients = new();
    private CancellationTokenSource? _streamCts;

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse());
    }
    
    public override async Task StreamAudio(AudioRequest request, IServerStreamWriter<AudioChunk> responseStream, ServerCallContext context)
    {
        var clientId = Guid.NewGuid().ToString();
        _clients.TryAdd(clientId, responseStream);

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, context.CancellationToken); // Клиент просто слушает
            }
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
        }
    }
    
    public override async Task<StreamResponse> StartStream(StreamRequest request, ServerCallContext context)
    {
        try
        {
            if (_streamCts != null)  
                return new StreamResponse
                {
                    Success = true,
                    Message = "Already started."
                };

            _ = Task.Run(async () =>
            {
                _streamCts = new CancellationTokenSource();
            
                var filePath = Path.Combine(AudioFolder, "1.mp3");
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Audio file {filePath} not found");
                
                await using var reader = File.OpenRead(filePath); // Читаем напрямую mp3
                var buffer = new byte[8192];
                long position = 0;

                while (!_streamCts.Token.IsCancellationRequested)
                {
                    var bytesRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), _streamCts.Token);
                    if (bytesRead == 0) break;

                    var chunk = new AudioChunk
                    {
                        Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
                        SampleRate = 48000, // можно оставить, если нужно
                        Position = position
                    };

                    foreach (var client in _clients.Values)
                    {
                        try { await client.WriteAsync(chunk); } catch { }
                    }

                    position += bytesRead;
                    await Task.Delay(50);
                }
            });
           

            return 
            (
                new StreamResponse
                {
                    Success = true,
                    Message = $"Broadcast started with track"
                }
            );
        }
        catch (Exception ex)
        {
            _streamCts = null;
            return (new StreamResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    public override Task<StreamResponse> StopStream(StopRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StreamResponse
        {
            Success = true,
            Message = "Stream stopped"
        });
    }

    public override Task<StreamStatus> GetStreamStatus(StatusRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StreamStatus
        {
            IsPlaying = true,
            CurrentTrack = "current",
            ListenersCount = _clients.Count
        });
    }
}