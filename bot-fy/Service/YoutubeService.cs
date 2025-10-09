using bot_fy.Extensions.Discord;
using DSharpPlus.Entities;
using SpotifyAPI.Web;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using System.Linq;
using System.Net;

namespace bot_fy.Service
{
    public class YoutubeService
    {
        private readonly YoutubeClient youtube;
        private readonly YoutubeAuthService authService;
        private readonly int MAX_RESULTS_PLAYLIST = 200;

        public YoutubeService()
        {
            youtube = new YoutubeClient();
            authService = new YoutubeAuthService();
        }

        /// <summary>
        /// Inicializa o serviço com autenticação (cookies do YouTube)
        /// </summary>
        public async Task InitializeWithAuthenticationAsync()
        {
            try
            {
                var cookies = await authService.GetYoutubeCookiesAsync();
                if (cookies.Any())
                {
                    // Recria o cliente com os cookies
                    var authenticatedYoutube = new YoutubeClient(cookies);
                    // Substitui a instância atual
                    typeof(YoutubeService).GetField("youtube", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, authenticatedYoutube);
                    Console.WriteLine("YouTubeService inicializado com autenticação.");
                }
                else
                {
                    Console.WriteLine("Nenhum cookie válido encontrado. Usando modo não autenticado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar autenticação: {ex.Message}");
            }
        }

        /// <summary>
        /// Força uma nova autenticação
        /// </summary>
        public async Task ReauthenticateAsync()
        {
            try
            {
                var cookies = await authService.ReauthenticateAsync();
                if (cookies.Any())
                {
                    var authenticatedYoutube = new YoutubeClient(cookies);
                    typeof(YoutubeService).GetField("youtube", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, authenticatedYoutube);
                    Console.WriteLine("Reautenticação realizada com sucesso.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na reautenticação: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se há autenticação válida
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            return await authService.HasValidCookiesAsync();
        }

        /// <summary>
        /// Limpa a autenticação
        /// </summary>
        public async Task ClearAuthenticationAsync()
        {
            await authService.ClearCookiesAsync();
            // Recria o cliente sem cookies
            var unauthenticatedYoutube = new YoutubeClient();
            typeof(YoutubeService).GetField("youtube", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, unauthenticatedYoutube);
            Console.WriteLine("Autenticação removida.");
        }

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
                return new List<IVideo>() { result };
            }
            return new List<IVideo>();
        }

        private async Task<List<IVideo>> GetPlayListVideosAsync(string link, DiscordChannel channel)
        {
            try
            {
                Playlist playlist = await youtube.Playlists.GetAsync(link);
                IReadOnlyList<PlaylistVideo> videosSubset = await youtube.Playlists
                    .GetVideosAsync(playlist.Id)
                    .CollectAsync(MAX_RESULTS_PLAYLIST);

                await channel.SendNewPlaylistAsync(playlist, videosSubset);

                return new List<IVideo>(videosSubset);
            }
            catch (Exception ex) when (ex.Message.Contains("private") || ex.Message.Contains("unavailable"))
            {
                // Se falhar por ser privada, tenta com autenticação
                if (!await IsAuthenticatedAsync())
                {
                    await channel.SendMessageAsync("Esta playlist é privada. Iniciando processo de autenticação...");
                    await InitializeWithAuthenticationAsync();
                    
                    // Tenta novamente com autenticação
                    try
                    {
                        Playlist playlist = await youtube.Playlists.GetAsync(link);
                        IReadOnlyList<PlaylistVideo> videosSubset = await youtube.Playlists
                            .GetVideosAsync(playlist.Id)
                            .CollectAsync(MAX_RESULTS_PLAYLIST);

                        await channel.SendNewPlaylistAsync(playlist, videosSubset);
                        return new List<IVideo>(videosSubset);
                    }
                    catch
                    {
                        await channel.SendMessageAsync("Não foi possível acessar esta playlist mesmo com autenticação.");
                        return new List<IVideo>();
                    }
                }
                
                await channel.SendMessageAsync("Não foi possível acessar esta playlist.");
                return new List<IVideo>();
            }
        }

        private async Task<List<IVideo>> GetVideosAsync(string link, DiscordChannel channel)
        {
            try
            {
                IVideo video = await youtube.Videos.GetAsync(link);
                await channel.SendNewMusicAsync(video);
                return new List<IVideo> { video };
            }
            catch (Exception ex) when (ex.Message.Contains("private") || ex.Message.Contains("unavailable"))
            {
                // Se falhar por ser privado, tenta com autenticação
                if (!await IsAuthenticatedAsync())
                {
                    await channel.SendMessageAsync("Este vídeo é privado. Iniciando processo de autenticação...");
                    await InitializeWithAuthenticationAsync();
                    
                    // Tenta novamente com autenticação
                    try
                    {
                        IVideo video = await youtube.Videos.GetAsync(link);
                        await channel.SendNewMusicAsync(video);
                        return new List<IVideo> { video };
                    }
                    catch
                    {
                        await channel.SendMessageAsync("Não foi possível acessar este vídeo mesmo com autenticação.");
                        return new List<IVideo>();
                    }
                }
                
                await channel.SendMessageAsync("Não foi possível acessar este vídeo.");
                return new List<IVideo>();
            }
        }

        private async Task<List<IVideo>> GetPlaylistSpotify(string link, DiscordChannel channel)
        {
            var spotifyAuth = new ClientCredentialsRequest(Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID")!, Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET")!);
            var spotifyResponse = await new OAuthClient().RequestToken(spotifyAuth);

            string spotifyAccessToken = spotifyResponse.AccessToken;
            var spotify = new SpotifyClient(spotifyAccessToken);

            string playlistId = link.Split("/").Last();
            playlistId = playlistId.Split("?").First();

            FullPlaylist playlist = await spotify.Playlists.Get(playlistId);
            var playlistTracks = await spotify.Playlists.GetItems(playlistId);

            List<string> musics = new();
            musics.AddRange(from FullTrack track in playlistTracks.Items!.Select(item => item.Track) select $"{track.Name} - {track.Artists.First().Name}");

            await channel.SendNewPlaylistSpotify(playlist);

            List<IVideo> videos = new();
            var tasks = musics.Select(music =>
                Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var result in youtube.Search.GetVideosAsync(music))
                        {
                            Console.Write(result.Title);
                            videos.Add(result);
                            break;
                        }
                    }
                    catch { }
                })
            );

            await Task.WhenAll(tasks);

            return videos;
        }

        public async Task<Video> GetVideoAsync(string video_id)
        {
            return await youtube.Videos.GetAsync(video_id);
        }
    }
}