using Serilog;
using System.Diagnostics;
using YoutubeExplode;
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
            ProcessStartInfo processStartInfo = GetProcessStartInfo(url);
            Process ffmpeg = StartFfmpegProcess(processStartInfo, cancellationToken);

            return ffmpeg;
        }

        private ProcessStartInfo GetProcessStartInfo(string url)
        {
            return new()
            {
                FileName = GetFfmpeg(),
                Arguments = $@"-reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 2 -timeout 10000000 -i ""{url}"" -vn -q:a 2 -ac 2 -f s16le -ar 48000 -b:a 96k pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        }

        private Process StartFfmpegProcess(ProcessStartInfo processStartInfo, CancellationToken cancellationToken)
        {
            Process ffmpeg = Process.Start(processStartInfo) ?? throw new InvalidOperationException("FFmpeg não foi iniciado");

            ffmpeg.ErrorDataReceived += (s, e) =>
            {
                Log.Error(e.Data);
            };

            //ffmpeg.BeginErrorReadLine();
            
            
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

        private async Task<string> GetAudioUrl(string url)
        {
            try
            {
                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                return streamInfo.Url;
            }
            catch (VideoUnplayableException)
            {
                return await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
            }
        }
    }
}
