using DSharpPlus.Entities;

namespace BotFy.Components
{
    public class DiscordMessages
    {
        public DiscordMessageBuilder GetMessageBuilderWithControls()
        {
            DiscordMessageBuilder builder = new()
            {

            };

            DiscordButtonComponent buttonSkip = new(DiscordButtonStyle.Primary, "skip", "Skip", false, new DiscordComponentEmoji("🎵"));
            DiscordButtonComponent buttonStop = new(DiscordButtonStyle.Secondary, "stop", "Stop", false, new DiscordComponentEmoji("⏹️"));
            DiscordButtonComponent buttonQueue = new(DiscordButtonStyle.Success, "queue", "Queue", false, new DiscordComponentEmoji("⏳"));
            DiscordButtonComponent buttonShuffle = new(DiscordButtonStyle.Success, "shuffle", "Shuffle", false, new DiscordComponentEmoji("🔀"));

            builder.AddComponents(buttonSkip, buttonStop, buttonQueue, buttonShuffle);

            return builder;
        }
    }
}
