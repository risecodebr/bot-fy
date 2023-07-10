using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace bot_fy.Service
{
    public class AudioService
    {
        private readonly YoutubeClient youtube = new();

        public Stream ConvertAudioToPcm(string filePath)
        {
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = GetFfmpeg(),
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            return ffmpeg.StandardOutput.BaseStream;
        }

        public string GetFfmpeg()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return $"{Directory.GetCurrentDirectory()}//ffmpeg//ffmpeg.exe";
            }
            return "ffmpeg";
        }

        public async Task DownloadAudioAsync(string video_id, string filePath)
        {
            await youtube.Videos.DownloadAsync(video_id, filePath, o => o
                                .SetContainer(Container.Mp3)
                                .SetPreset(ConversionPreset.UltraFast)
                                .SetFFmpegPath(GetFfmpeg()));
        }
    }
}