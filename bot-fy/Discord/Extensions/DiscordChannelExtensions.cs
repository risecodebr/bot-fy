using DSharpPlus.Entities;
using YoutubeExplode;

namespace bot_fy.Service
{
    public static class NotificationsDiscordChannelExtensions
    {
        private static readonly YoutubeClient youtube = new();

        public static async Task<DiscordMessage> SendNewMusicAsync(this DiscordChannel channel, string video_id)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Nova música adicionada",
                Description = $"[Clique aqui para ouvir](https://www.youtube.com/watch?v={video_id})",
                Color = DiscordColor.Green
            };
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewPlaylistAsync(this DiscordChannel channel, string playlist_id)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Nova playlist adicionada",
                Description = $"[Clique aqui para ouvir](https://www.youtube.com/playlist?list={playlist_id})",
                Color = DiscordColor.Green
            };
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewMusicPlayAsync(this DiscordChannel channel, string video_id)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Nova musica tocando...",
                Description = $"[Clique aqui para ouvir](https://www.youtube.com/watch?v={video_id})",
                Color = DiscordColor.Green
            };
            return await channel.SendMessageAsync(embed);
        }
    }
}