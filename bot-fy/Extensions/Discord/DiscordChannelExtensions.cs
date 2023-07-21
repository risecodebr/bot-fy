using DSharpPlus.Entities;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace bot_fy.Extensions.Discord
{
    public static class NotificationsDiscordChannelExtensions
    {
        private static readonly YoutubeClient youtube = new();

        public static async Task<DiscordMessage> SendNewMusicAsync(this DiscordChannel channel, IVideo video)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = $"Adicionado a fila {video.Title}",
                Color = DiscordColor.Green,
                Url = video.Url
            };
            embed.AddField("Canal", video.Author.ChannelTitle);
            embed.AddField("Tempo", $"{video.Duration.ToStringTime()}", true);
            embed.WithThumbnail(video.Thumbnails.OrderByDescending(p => p.Resolution.Height).First().Url);
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewPlaylistAsync(this DiscordChannel channel, Playlist playlist, IReadOnlyList<PlaylistVideo> videos)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = $"Adicionado a fila {playlist.Title}",
                Color = DiscordColor.Green,
                Url = playlist.Url
            };
            embed.AddField("Quantidade de músicas", videos.Count.ToString());
            embed.AddField("Duração", $"{videos.Sum(p => p.Duration).ToStringTime()}", true);
            embed.WithThumbnail(playlist.Thumbnails.OrderByDescending(p => p.Resolution.Height).First().Url);
            return await channel.SendMessageAsync(embed);
        }

        public static async Task<DiscordMessage> SendNewMusicPlayAsync(this DiscordChannel channel, string video_id)
        {
            Video video = await youtube.Videos.GetAsync(video_id);
            DiscordEmbedBuilder embed = new()
            {
                Title = $"Tocando {video.Title}",
                Color = DiscordColor.Green,
                Url = video.Url
            };
            embed.AddField("Canal", video.Author.ChannelTitle);
            embed.AddField("Tempo", $"{video.Duration.ToStringTime()}", true);

            embed.WithThumbnail(video.Thumbnails.OrderByDescending(p => p.Resolution.Height).First().Url);
            return await channel.SendMessageAsync(embed);
        }
    }
}