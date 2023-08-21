using bot_fy.Extensions.Discord;
using DSharpPlus.Entities;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace bot_fy.Service
{
    public class YoutubeService
    {
        private readonly YoutubeClient youtube = new();
        private readonly int MAX_RESULTS_PLAYLIST = 200;

        public async Task<List<IVideo>> GetResultsAsync(string termo, DiscordChannel channel)
        {
            if (!termo.Contains("youtu.be/") && !termo.Contains("youtube.com"))
            {
                return await GetVideoByTermAsync(termo, channel);
            }
            if (termo.Contains("playlist?list"))
            {
                return await GetPlayListVideosAsync(termo, channel);
            }
            return await GetVideosAsync(termo, channel);
        }

        public async Task<List<IVideo>> GetVideoByTermAsync(string termo, DiscordChannel channel)
        {
            await foreach (VideoSearchResult result in youtube.Search.GetVideosAsync(termo))
            {
                await channel.SendNewMusicAsync(result);
                return new List<IVideo>() { result };
            }
            return new List<IVideo>();
        }

        private async Task<List<IVideo>> GetPlayListVideosAsync(string link, DiscordChannel channel)
        {
            Playlist playlist = await youtube.Playlists.GetAsync(link);
            IReadOnlyList<PlaylistVideo> videosSubset = await youtube.Playlists
                .GetVideosAsync(playlist.Id)
                .CollectAsync(MAX_RESULTS_PLAYLIST);

            await channel.SendNewPlaylistAsync(playlist, videosSubset);

            return new List<IVideo>(videosSubset);
        }

        private async Task<List<IVideo>> GetVideosAsync(string link, DiscordChannel channel)
        {
            IVideo video = await youtube.Videos.GetAsync(link);

            await channel.SendNewMusicAsync(video);

            return new List<IVideo> { video };
        }

        public async Task<IVideo> GetVideoAsync(string video_id)
        {
            return await youtube.Videos.GetAsync(video_id);
        }
    }
}