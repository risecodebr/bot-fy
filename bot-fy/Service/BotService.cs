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
                if (!File.Exists("ffmpeg/ffmpeg.exe"))
                {
                    Log.Information("Downloading ffmpeg");
                    HttpClient client = new();
                    byte[] file = await client.GetByteArrayAsync("https://cdn.discordapp.com/attachments/958820650410213436/1127666156627578940/ffmpeg.exe");
                    File.WriteAllBytes("ffmpeg/ffmpeg.exe", file);

                }
            }
            if (!Directory.Exists("music"))
            {
                Directory.CreateDirectory("music");
            }
            Directory.GetFiles("music").ToList().ForEach(File.Delete);
            return;
        }
    }
}