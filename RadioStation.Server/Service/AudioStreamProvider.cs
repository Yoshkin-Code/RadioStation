// using Google.Protobuf;
// using NAudio.Wave;
// using RadioStationServer.GrpcProto;
//
// namespace RadioStation.Server.Service;
//
// public class AudioStreamProvider : IDisposable
// {
//     private readonly string _audioFolder;
//     private AudioFileReader _currentReader;
//     private string _currentTrack;
//     private long _currentPosition;
//     private readonly object _syncLock = new object();
//
//     public event EventHandler<AudioChunk> OnChunkRead;
//
//     public AudioStreamProvider(string audioFolder)
//     {
//         _audioFolder = audioFolder;
//     }
//
//     public void StartStream(string trackName)
//     {
//         lock (_syncLock)
//         {
//             _currentTrack = trackName;
//             _currentPosition = 0;
//             InitializeReader();
//         }
//     }
//
//     public void StopStream()
//     {
//         lock (_syncLock)
//         {
//             _currentReader?.Dispose();
//             _currentReader = null;
//         }
//     }
//
//     public void ChangeTrack(string trackName)
//     {
//         lock (_syncLock)
//         {
//             _currentTrack = trackName;
//             _currentPosition = 0;
//             InitializeReader();
//         }
//     }
//
//     public void Seek(long position)
//     {
//         lock (_syncLock)
//         {
//             if (_currentReader != null)
//             {
//                 _currentPosition = position;
//                 _currentReader.Position = _currentPosition;
//             }
//         }
//     }
//
//     public async Task StreamAudioAsync(CancellationToken cancellationToken)
//     {
//         while (!cancellationToken.IsCancellationRequested)
//         {
//             byte[] buffer;
//             long position;
//             int bytesRead;
//             int sampleRate;
//
//             lock (_syncLock)
//             {
//                 if (_currentReader == null)
//                 {
//                     await Task.Delay(100, cancellationToken);
//                     continue;
//                 }
//
//                 buffer = new byte[8192];
//                 bytesRead = _currentReader.Read(buffer, 0, buffer.Length);
//                 position = _currentReader.Position;
//                 sampleRate = _currentReader.WaveFormat.SampleRate;
//                 _currentPosition = position;
//             }
//
//             if (bytesRead == 0)
//             {
//                 // Конец трека
//                 await Task.Delay(100, cancellationToken);
//                 continue;
//             }
//
//             var chunk = new AudioChunk
//             {
//                 Data = ByteString.CopyFrom(buffer, 0, bytesRead),
//                 SampleRate = sampleRate,
//                 Position = position
//             };
//
//             OnChunkRead?.Invoke(this, chunk);
//             await Task.Delay(50, cancellationToken); // Регулируем скорость потока
//         }
//     }
//
//     public AudioChunk GetCurrentChunkSnapshot()
//     {
//         lock (_syncLock)
//         {
//             if (_currentReader == null || string.IsNullOrEmpty(_currentTrack))
//                 return null;
//
//             var filePath = Path.Combine(_audioFolder, _currentTrack);
//             if (!File.Exists(filePath))
//                 return null;
//
//             using var snapshotReader = new AudioFileReader(filePath)
//             {
//                 Position = _currentPosition
//             };
//
//             var buffer = new byte[8192];
//             var bytesRead = snapshotReader.Read(buffer, 0, buffer.Length);
//             
//             return new AudioChunk
//             {
//                 Data = ByteString.CopyFrom(buffer, 0, bytesRead),
//                 SampleRate = snapshotReader.WaveFormat.SampleRate,
//                 Position = _currentPosition
//             };
//         }
//     }
//
//     private void InitializeReader()
//     {
//         _currentReader?.Dispose();
//         
//         var filePath = Path.Combine(_audioFolder, _currentTrack);
//         if (!File.Exists(filePath))
//             throw new FileNotFoundException($"Audio file {filePath} not found");
//
//         _currentReader = new AudioFileReader(filePath)
//         {
//             Position = _currentPosition
//         };
//     }
//
//     public void Dispose()
//     {
//         _currentReader?.Dispose();
//     }
// }
