using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;
using System.Net;

namespace bot_fy.Service
{
    public class AudioService
    {
        private YoutubeClient youtube;
        private readonly YoutubeAuthService authService;

        public AudioService()
        {
            youtube = new YoutubeClient();
            authService = new YoutubeAuthService();
        }

        /// <summary>
        /// Inicializa o serviço com autenticação se disponível
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (await authService.HasValidCookiesAsync())
                {
                    var cookies = await authService.GetYoutubeCookiesAsync();
                    if (cookies.Any())
                    {
                        youtube = new YoutubeClient(cookies);
                        Console.WriteLine("AudioService inicializado com autenticação.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar AudioService com autenticação: {ex.Message}");
            }
        }

        public async Task<Process> ConvertAudioToPcm(string url_music, CancellationToken cancellationToken)
        {
            string url = await GetAudioUrl(url_music);
            Process? ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = GetFfmpeg(),
                Arguments = $@"-reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 2 -timeout 10000000 -i ""{url}"" -vn -q:a 2 -ac 2 -f s16le -ar 48000 -loglevel error -b:a 96k pipe:1",
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
            catch (Exception ex) when (ex.Message.Contains("private") || ex.Message.Contains("unavailable"))
            {
                // Tenta com autenticação se disponível
                try
                {
                    if (await authService.HasValidCookiesAsync())
                    {
                        var cookies = await authService.GetYoutubeCookiesAsync();
                        youtube = new YoutubeClient(cookies);
                        
                        StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                        IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                        return streamInfo.Url;
                    }
                }
                catch
                {
                    // Se mesmo com autenticação falhar, relança a exceção original
                }
                
                throw;
            }
        }
    }
}
