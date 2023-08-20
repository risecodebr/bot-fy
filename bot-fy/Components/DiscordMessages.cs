using DSharpPlus;
using DSharpPlus.Entities;

namespace bot_fy.Components
{
    public class DiscordMessages
    {
        public DiscordMessageBuilder GetMessageBuilderWithControls(DiscordEmbedBuilder embed)
        {
            DiscordMessageBuilder builder = new()
            {
                Embed = embed
            };

            DiscordButtonComponent buttonSkip = new(ButtonStyle.Primary, "skip", "Skip", false, new DiscordComponentEmoji("🎵"));
            DiscordButtonComponent buttonStop = new(ButtonStyle.Secondary, "stop", "Stop", false, new DiscordComponentEmoji("⏹️"));
            DiscordButtonComponent buttonQueue = new(ButtonStyle.Success, "queue", "Queue", false, new DiscordComponentEmoji("⏳"));
            DiscordButtonComponent buttonShuffle = new(ButtonStyle.Success, "shuffle", "Shuffle", false, new DiscordComponentEmoji("🔀"));

            builder.AddComponents(buttonSkip, buttonStop, buttonQueue, buttonShuffle);

            return builder;
        }
    }
}
