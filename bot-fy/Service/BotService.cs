using Serilog;

namespace bot_fy.Service
{
    public class BotService
    {
        public static async Task CreatePathsAsync()
        {
            if (!Directory.Exists("ffmpeg"))
            {
                Directory.CreateDirectory("ffmpeg");
                Log.Information("Created ffmpeg directory");
            }
            if (!File.Exists("ffmpeg/ffmpeg.exe") && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Log.Information("Downloading ffmpeg");
                HttpClient client = new();
                byte[] file = await client.GetByteArrayAsync(Environment.GetEnvironmentVariable("URL_FFMPEG")!);
                File.WriteAllBytes("ffmpeg/ffmpeg.exe", file);
                Log.Information("Downloaded ffmpeg");
            }

            return;
        }
    }
}