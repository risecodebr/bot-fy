using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace bot_fy.Service
{
    public class AudioService
    {
        private readonly YoutubeClient youtube = new();

        public async Task<Process> ConvertAudioToPcm(string url_music, CancellationToken cancellationToken)
        {
            string url = await GetAudioUrl(url_music);
            Process? ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = GetFfmpeg(),
                Arguments = $@"-reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5 -i ""{url}"" -ac 2 -f s16le -ar 48000 -loglevel error -b:a 96k pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            ffmpeg.Exited += (s, e) => ffmpeg.Dispose();

            cancellationToken.Register(ffmpeg.Kill);

            return ffmpeg;
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

        private async Task<string> GetAudioUrl(string url)
        {
            try
            {
                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                return streamInfo.Url;
            }
            catch(VideoUnplayableException)
            {
                return await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
            }
        }
    }
}