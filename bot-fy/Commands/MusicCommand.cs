using bot_fy.Discord.Extensions;
using bot_fy.Service;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace bot_fy.Commands
{
    public class MusicCommand : ApplicationCommandModule
    {
        private static Dictionary<ulong, Queue<string>> track = new();
        private static Dictionary<ulong, string> directory = new();
        private readonly YoutubeService youtubeService = new();
        private readonly AudioService audioService = new();

        private static event EventHandler<ulong> MusicSkipped;

        [SlashCommand("play", "Reproduza sua musica ou playlist")]
        public async Task Play(InteractionContext ctx, [Option("link", "Link da musica, playlist ou mix do Youtube")] string termo)
        {
            if(!await ctx.ValidateChannels()) return;

            List<string> videos = await youtubeService.GetResultsAsync(termo, ctx.Channel);
            if (!videos.Any())
            {
                await ctx.CreateResponseAsync("Nenhum Video Encontrado"); 
                return;
            }

            if (!track.ContainsKey(ctx.Guild.Id))
            {
                track.Add(ctx.Guild.Id, new Queue<string>());
            }

            videos.ForEach(v => track[ctx.Guild.Id].Enqueue(v));

            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            VoiceNextConnection connection = vnext.GetConnection(ctx.Guild);

            VoiceTransmitSink transmit;

            if (connection == null)
            {
                DiscordChannel channel = ctx.Member.VoiceState?.Channel;
                connection = await channel.ConnectAsync();
            }
            else
            {
                if (videos.Count == 1)
                {
                    Video video = await youtubeService.GetVideoAsync(videos[0]);
                    await ctx.Channel.SendNewMusicAsync(video);
                }
                else
                {
                    Playlist playlist = await youtubeService.GetPlaylistAsync(termo);
                    await ctx.Channel.SendNewPlaylistAsync(playlist);
                }
                return;
            }
            transmit = connection.GetTransmitSink();

            connection.UserLeft += async (v, u) =>
            {
                if (u.User.Id == ctx.Guild.CurrentMember.Id)
                {
                    await ctx.Channel.SendMessageAsync("Saindo do canal de voz");
                }
            };

            MusicSkipped += (sender, guildId) =>
            {
                if (guildId == ctx.Guild.Id)
                {
                    Console.WriteLine("Musica pulada");
                }
            };

            for (int i = 0; i < track[ctx.Guild.Id].Count; i = 0)
            {
                string videoid = track[ctx.Guild.Id].Dequeue();
                directory[ctx.Guild.Id] = $"{Directory.GetCurrentDirectory()}\\music\\{ctx.Guild.Id}-{ctx.User.Id}-{videoid}.mp3";
                Video video = await youtubeService.GetVideoAsync(videoid);
                await ctx.Channel.SendNewMusicPlayAsync(video);
                await audioService.DownloadAudioAsync(videoid, directory[ctx.Guild.Id]);

                Stream pcm = audioService.ConvertAudioToPcm(directory[ctx.Guild.Id]);
                await pcm.CopyToAsync(transmit, null);

                File.Delete(directory[ctx.Guild.Id]);
                directory[ctx.Guild.Id] = "";

                await pcm.DisposeAsync();
            }
            connection.Disconnect();

        }

        [SlashCommand("skip", "Pule a musica atual")]
        public async Task Skip(InteractionContext ctx)
        {
           Console.WriteLine("Skip");
           MusicSkipped(this, ctx.Guild.Id);
           Console.WriteLine("Skip2");
           await ctx.CreateResponseAsync("Musica pulada");
        }
    }
}