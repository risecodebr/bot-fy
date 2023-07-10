namespace bot_fy.Service
{
    public class BotService
    {
        public static Task CreatePathsAsync()
        {
            if (!Directory.Exists("ffmpeg"))
            {
                Directory.CreateDirectory("ffmpeg");
            }
            if (!Directory.Exists("music"))
            {
                Directory.CreateDirectory("music");
            }
            Directory.GetFiles("music").ToList().ForEach(File.Delete);
            return Task.CompletedTask;
        }
    }
}