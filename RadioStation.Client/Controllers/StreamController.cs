using System.Net.WebSockets;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using RadioStation.Client.Services;
using RadioStationServer.GrpcProto;

namespace RadioStation.Client.Controllers;

[Route("audio")]
public class StreamController(
    RadioControl.RadioControlClient grpcClient,
    ILogger<StreamController> logger)
    : ControllerBase
{
    [HttpGet("stream-socket")]
    public async Task GetStreamViaWebSocket(CancellationToken cancellationToken)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var status = await GetStatusAsync();
        // if (!status.IsPlaying)
        // {
        //     HttpContext.Response.StatusCode = 204;
        //     return;
        // }
            
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var grpcStream = GetAudioStreamAsync("1");

        try
        {
            await foreach (var chunk in grpcStream.ResponseStream.ReadAllAsync(cancellationToken))
            {
                if (webSocket.CloseStatus.HasValue) 
                    break;
                
                await webSocket.SendAsync(
                    chunk.Data.ToByteArray(),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None);
                
                // if (webSocket.CloseStatus.HasValue)
                //     break;
                //
                // var pcmData = chunk.Data.ToByteArray();
                // var wavData = ConvertFloat32ToWav(pcmData, 48000, 2);
                // await webSocket.SendAsync(wavData, WebSocketMessageType.Binary, true, cancellationToken);

            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while streaming audio");
        }
        finally
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        }
    }
    
    public AsyncUnaryCall<StreamStatus> GetStatusAsync() =>
        grpcClient.GetStreamStatusAsync(new StatusRequest());
       
    public AsyncServerStreamingCall<AudioChunk> GetAudioStreamAsync(string userId)
    {
        var request = new AudioRequest { UserId = 1 };
        var call = grpcClient.StreamAudio(request);
        return call;
        
        // var outputStream = new MemoryStream();
        //
        // await foreach (var chunk in call.ResponseStream.ReadAllAsync())
        // {
        //     await outputStream.WriteAsync(chunk.Data.ToByteArray());
        // }
        //
        // outputStream.Position = 0;
        // return outputStream;
    }
}