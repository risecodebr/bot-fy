using bot_fy.Extensions.Discord;
using DSharpPlus.Entities;
using SpotifyAPI.Web;
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

        public async Task<List<IVideo>> GetResultsAsync(string termo, DiscordChannel channel)
        {
            if(termo.Contains("spotify.com/playlist/"))
            {
                return await GetPlaylistSpotify(termo, channel);
            }
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
                return new() { result };
            }
            return new();
        }

        private async Task<List<IVideo>> GetPlayListVideosAsync(string link, DiscordChannel channel)
        {
            Playlist playlist = await youtube.Playlists.GetAsync(link);
            IReadOnlyList<PlaylistVideo> videosSubset = await youtube.Playlists
                .GetVideosAsync(playlist.Id)
                .CollectAsync();

            await channel.SendNewPlaylistAsync(playlist, videosSubset);

            return new(videosSubset);
        }

        private async Task<List<IVideo>> GetVideosAsync(string link, DiscordChannel channel)
        {
            IVideo video = await youtube.Videos.GetAsync(link);

            await channel.SendNewMusicAsync(video);

            return new() { video };
        }

        private async Task<List<IVideo>> GetPlaylistSpotify(string link, DiscordChannel channel)
        {
            SpotifyClient spotify = await GetSpotifyClient();

            string playlistId = ParsePlaylistId(link);

            FullPlaylist playlist = await spotify.Playlists.Get(playlistId);
            await channel.SendNewPlaylistSpotify(playlist);

            var playlistTracks = await spotify.Playlists.GetItems(playlistId);
            var itens = await spotify.PaginateAll(playlistTracks);

            List<string> musics = ConvertSpotifyTrackToTermSearch(itens);

            List<IVideo> videos = await ProcessTracksSpotify(musics);

            return videos;
        }

        private List<string> ConvertSpotifyTrackToTermSearch(IList<PlaylistTrack<IPlayableItem>> tracks)
        {
            return tracks.Select(track => track.Track as FullTrack)
                .Select(p => $"{p.Name} - {p.Artists[0].Name}")
                .ToList();
        }

        private async Task<List<IVideo>> ProcessTracksSpotify(List<string> tracks)
        {
            List<IVideo> tracksResult = new();
            var tasks = tracks.Select(music =>
                Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var result in youtube.Search.GetVideosAsync(music))
                        {
                            tracksResult.Add(result);
                            break;
                        }
                    }
                    catch { }
                })
            );

            await Task.WhenAll(tasks);

            return tracksResult;
        }

        private string ParsePlaylistId(string link)
        {
            string playlistId = link.Split("/").Last();
            playlistId = playlistId.Split("?").First();
            return playlistId;
        }

        private async Task<SpotifyClient> GetSpotifyClient()
        {
            var spotifyAuth = new ClientCredentialsRequest(
                Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID")!, 
                Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET")!);

            var spotifyResponse = await new OAuthClient().RequestToken(spotifyAuth);

            string spotifyAccessToken = spotifyResponse.AccessToken;
            return new(spotifyAccessToken);
        }

        public async Task<Video> GetVideoAsync(string video_id)
        {
            return await youtube.Videos.GetAsync(video_id);
        }
    }
}