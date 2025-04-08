using Grpc.Core;
using RadioStationServer.GrpcProto;

namespace RadioStation.Client.Services;

public class GrpcAudioStream(IAsyncStreamReader<AudioChunk> responseStream) : Stream
{
    private MemoryStream _currentChunk = new();

    public override async Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_currentChunk.Position >= _currentChunk.Length)
        {
            if (!await responseStream.MoveNext(cancellationToken))
                return 0;

            _currentChunk = new MemoryStream(
                responseStream.Current.Data.ToByteArray());
        }

        return await _currentChunk.ReadAsync(buffer, offset, count, cancellationToken);
    }

    // Остальные обязательные методы Stream
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get; set; }
    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) 
        => throw new NotSupportedException("Use async version");
    public override long Seek(long offset, SeekOrigin origin) 
        => throw new NotSupportedException();
    public override void SetLength(long value) 
        => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) 
        => throw new NotSupportedException();
}
