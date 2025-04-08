using Grpc.Core;
using RadioStationServer.GrpcProto;

namespace RadioStation.Client.Services;

public class GrpcAudioStreamClient(ILogger<GrpcAudioStreamClient> logger, RadioControl.RadioControlClient grpcClient)
{
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