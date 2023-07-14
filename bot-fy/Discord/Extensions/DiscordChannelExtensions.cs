using DSharpPlus.Entities;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace bot_fy.Discord.Extensions
{
    public static class NotificationsDiscordChannelExtensions
    {
        private static readonly YoutubeClient youtube = new();

        public static async Task<DiscordMessage> SendNewMusicAsync(this DiscordChannel channel, Video video)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = video.Title,
                Description = $"Tempo: `{video.Duration.Value.Minutes}` minutos",
                Color = DiscordColor.Green
            };
            embed.Url = video.Url;
            embed.WithThumbnail(video.Thumbnails.First().Url);
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewPlaylistAsync(this DiscordChannel channel, Playlist playlist)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = playlist.Title,
                Color = DiscordColor.Green
            };
            embed.Url = playlist.Url;
            embed.WithThumbnail(playlist.Thumbnails.First().Url);
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewMusicPlayAsync(this DiscordChannel channel, Video video)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = video.Title,
                Description = $"Tempo: `{video.Duration.Value.Minutes}` minutos",
                Color = DiscordColor.Green
            };
            embed.Url = video.Url;
            embed.WithThumbnail(video.Thumbnails.First().Url);
            return await channel.SendMessageAsync(embed);
        }
    }
}