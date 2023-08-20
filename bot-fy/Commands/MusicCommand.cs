using bot_fy.Extensions;
using bot_fy.Extensions.Discord;
using bot_fy.Service;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using YoutubeExplode.Videos;

namespace bot_fy.Commands
{
    public class MusicCommand : ApplicationCommandModule
    {
        private static readonly Dictionary<ulong, Queue<IVideo>> track = new();
        private static readonly Dictionary<ulong, string> directory = new();
        private readonly YoutubeService youtubeService = new();
        private readonly AudioService audioService = new();

        private static event EventHandler<ulong> OnMusicSkipped;
        private static event EventHandler<ulong> OnMusicStopped;

        [SlashCommand("play", "Reproduza sua musica ou playlist")]
        public async Task Play(InteractionContext ctx, [Option("link", "Link da musica, playlist ou mix do Youtube")] string termo)
        {
            if (!await ctx.ValidateChannels()) return;

            await ctx.CreateResponseAsync("Buscando...");

            List<IVideo> videos = await youtubeService.GetResultsAsync(termo, ctx.Channel);
            if (!videos.Any())
            {
                await ctx.CreateResponseAsync("Nenhum Video Encontrado");
                return;
            }

            if (!track.ContainsKey(ctx.Guild.Id))
            {
                track.Add(ctx.Guild.Id, new Queue<IVideo>());
            }

            videos.ForEach(v => track[ctx.Guild.Id].Enqueue(v));

            VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
            VoiceNextConnection connection = vnext.GetConnection(ctx.Guild);

            if (connection == null)
            {
                DiscordChannel? channel = ctx.Member.VoiceState?.Channel;
                connection = await channel.ConnectAsync();
            }
            else
            {
                return;
            }
            VoiceTransmitSink transmit = connection.GetTransmitSink();
            CancellationTokenSource cancellationToken = new();
            CancellationToken token = cancellationToken.Token;

            connection.UserLeft += async (v, u) =>
            {
                if (u.User.Id == ctx.Guild.CurrentMember.Id)
                {
                    track[ctx.Guild.Id].Clear();
                    cancellationToken.Cancel();
                    await ctx.Channel.SendMessageAsync("Saindo do canal de voz");
                }
            };

            OnMusicSkipped += (v, g) =>
            {
                if (g == ctx.Guild.Id)
                {
                    cancellationToken.Cancel();
                }
            };

            OnMusicStopped += (v, g) =>
            {
                if (g == ctx.Guild.Id)
                {
                    cancellationToken.Cancel();
                    track[g].Clear();
                    return;
                }
            };

            for (int i = 0; i < track[ctx.Guild.Id].Count; i = 0)
            {
                cancellationToken = new();
                token = cancellationToken.Token;
                await Task.Run(async () =>
                {
                    IVideo video = track[ctx.Guild.Id].Dequeue();
                    directory[ctx.Guild.Id] = $"{Directory.GetCurrentDirectory()}\\music\\{ctx.Guild.Id}-{ctx.User.Id}-{video.Id}.mp3";
                    await audioService.DownloadAudioAsync(video.Id, directory[ctx.Guild.Id]);
                    DiscordMessage message = await ctx.Channel.SendNewMusicPlayAsync(video.Id);
                    Stream pcm = null;
                    try
                    {
                        Process process = audioService.ConvertAudioToPcm(directory[ctx.Guild.Id], token);
                        token.Register(process.Kill);
                        pcm = process.StandardOutput.BaseStream;
                        await pcm.CopyToAsync(transmit, null, token);
                        await pcm.DisposeAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        await pcm.DisposeAsync();
                    }

                    File.Delete(directory[ctx.Guild.Id]);
                    directory[ctx.Guild.Id] = "";

                    await message.DeleteAsync();
                });
            }
            connection.Disconnect();

        }

        [SlashCommand("shuffle", "Deixa a fila de musicas aleatoria")]
        public async Task Shuffle(InteractionContext ctx)
        {
            if (!track.ContainsKey(ctx.Guild.Id))
            {
                await ctx.CreateResponseAsync("Nenhuma musica na fila");
                return;
            }
            Shuffle(ctx.Guild.Id);
            await ctx.CreateResponseAsync("Fila embaralhada");
        }

        [SlashCommand("queue", "Mostra a fila de musicas")]
        public async Task Queue(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("Buscando...");

            if (!track.ContainsKey(ctx.Guild.Id))
            {
                await ctx.CreateResponseAsync("Nenhuma musica na fila");
                return;
            }

            await ctx.Channel.SendPaginatedMusicsAsync(ctx.User, track[ctx.Guild.Id]);
        }

        [SlashCommand("clear", "Limpa a fila de musicas")]
        public async Task Clear(InteractionContext ctx)
        {
            if (!track.ContainsKey(ctx.Guild.Id))
            {
                await ctx.CreateResponseAsync("Nenhuma musica na fila");
                return;
            }
            track[ctx.Guild.Id].Clear();
            await ctx.CreateResponseAsync("Fila limpa");
        }

        [SlashCommand("skip", "Pule a musica atual")]
        public async Task Skip(InteractionContext ctx)
        {
            if (!track.ContainsKey(ctx.Guild.Id))
            {
                await ctx.CreateResponseAsync("Nenhuma musica na fila");
                return;
            }
            Skip(ctx.Guild.Id);
            await ctx.CreateResponseAsync("Musica pulada (eu espero) ");
        }

        public static void Skip(ulong guildId)
        {
            OnMusicSkipped.Invoke(null, guildId);
        }
        public static void Stop(ulong guildId)
        {
            OnMusicStopped.Invoke(null, guildId);
        }
        public static void Shuffle(ulong guildId)
        {
            track[guildId] = track[guildId].Shuffle();
        }
        public static IEnumerable<IVideo> GetQueue(ulong guildId)
        {
            return track[guildId];
        }
    }
}