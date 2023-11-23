using Serilog;

namespace bot_fy.Service;

public class BotService
{
    public static async Task CreatePathsAsync()
    {
        if (!File.Exists("ffmpeg.exe") && Environment.OSVersion.Platform != PlatformID.Unix)
        {
            Log.Information("Downloading ffmpeg");
            HttpClient client = new();
            byte[] file = await client.GetByteArrayAsync(Environment.GetEnvironmentVariable("URL_FFMPEG")!);
            File.WriteAllBytes("ffmpeg.exe", file);
            Log.Information("Downloaded ffmpeg");
        }

        return;
    }
}