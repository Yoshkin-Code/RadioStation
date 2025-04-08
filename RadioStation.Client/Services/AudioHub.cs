using Grpc.Core;
using Microsoft.AspNetCore.SignalR;

namespace RadioStation.Client.Services;

public class AudioHub(GrpcAudioStreamClient grpcClient) : Hub
{
    public async Task StartStreaming(string userId)
    {
        var call = grpcClient.GetAudioStreamAsync(userId);

        await foreach (var chunk in call.ResponseStream.ReadAllAsync())
        {
            // Преобразуем ByteString в base64
            var base64 = Convert.ToBase64String(chunk.Data.ToByteArray());

            // Отправляем клиенту
            await Clients.Caller.SendAsync("ReceiveAudioChunk", base64, chunk.SampleRate);
        }
    }
}
