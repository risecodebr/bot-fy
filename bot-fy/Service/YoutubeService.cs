using DSharpPlus.Entities;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace bot_fy.Service
{
    public class YoutubeService
    {
        private readonly YoutubeClient youtube = new();
        private readonly int MAX_RESULTS_PLAYLIST = 200;

        public async Task<List<string>> GetResultsAsync(string termo, DiscordChannel channel)
        {
            if (!termo.Contains("youtu.be/") && !termo.Contains("youtube.com"))
            {
                return await GetVideoByTermAsync(termo, channel);
            }
            if (termo.Contains("playlist?list"))
            {
                return await GetPlayListVideosAsync(termo);
            }
            return await GetVideosAsync(termo);
        }

        public async Task<Playlist> GetPlaylistAsync(string playlist_url)
        {
            return await youtube.Playlists.GetAsync(playlist_url);
        }

        public async Task<List<string>> GetVideoByTermAsync(string termo, DiscordChannel channel)
        {
            await foreach (var result in youtube.Search.GetVideosAsync(termo))
            {
                return new List<string>() { result.Id.Value };
            }
            return new List<string>();
        }

        private async Task<List<string>> GetPlayListVideosAsync(string link)
        {
            Playlist playlist = await youtube.Playlists.GetAsync(link);
            IReadOnlyList<PlaylistVideo> videosSubset = await youtube.Playlists
                .GetVideosAsync(playlist.Id)
                .CollectAsync(MAX_RESULTS_PLAYLIST);

            return videosSubset.Select(v => v.Id.Value).ToList();
        }

        private async Task<List<string>> GetVideosAsync(string link)
        {
            var video = await youtube.Videos.GetAsync(link);
            return new List<string> { video.Id };
        }

        public async Task<Video> GetVideoAsync(string video_id)
        {
            return await youtube.Videos.GetAsync(video_id);
        }
    }
}