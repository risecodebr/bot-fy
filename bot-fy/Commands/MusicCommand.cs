using bot_fy.Extensions;
using bot_fy.Extensions.Discord;
using bot_fy.Service;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using YoutubeExplode.Videos;

namespace bot_fy.Commands
{
    public class MusicCommand : ApplicationCommandModule
    {
        private static readonly Dictionary<ulong, Queue<string>> track = new();
        private static readonly Dictionary<ulong, string> directory = new();
        private readonly YoutubeService youtubeService = new();
        private readonly AudioService audioService = new();

        private static event EventHandler<ulong> MusicSkipped;

        [SlashCommand("play", "Reproduza sua musica ou playlist")]
        public async Task Play(InteractionContext ctx, [Option("link", "Link da musica, playlist ou mix do Youtube")] string termo)
        {
            if (!await ctx.ValidateChannels()) return;

            await ctx.CreateResponseAsync("Buscando...");

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
                    await ctx.Channel.SendMessageAsync("Saindo do canal de voz");
                }
            };

            MusicSkipped += async (v, g) =>
            {
                if (g == ctx.Guild.Id)
                {
                    cancellationToken.Cancel();
                }
            };

            for (int i = 0; i < track[ctx.Guild.Id].Count; i = 0)
            {
                cancellationToken = new();
                token = cancellationToken.Token;
                await Task.Run(async () =>
                {
                    string video_id = track[ctx.Guild.Id].Dequeue();
                    directory[ctx.Guild.Id] = $"{Directory.GetCurrentDirectory()}\\music\\{ctx.Guild.Id}-{ctx.User.Id}-{video_id}.mp3";
                    await audioService.DownloadAudioAsync(video_id, directory[ctx.Guild.Id]);
                    DiscordMessage message = await ctx.Channel.SendNewMusicPlayAsync(video_id);
                    Stream pcm = null;
                    try
                    {
                        Process process = audioService.ConvertAudioToPcm(directory[ctx.Guild.Id], token);
                        token.Register(process.Kill);
                        pcm = process.StandardOutput.BaseStream;
                        Console.WriteLine("Iniciando musica");
                        await pcm.CopyToAsync(transmit, null, token);
                        Console.WriteLine("Musica finalizada");
                        await pcm.DisposeAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Musica finalizada");
                        await pcm.DisposeAsync();
                    }

                    File.Delete(directory[ctx.Guild.Id]);
                    Console.WriteLine("Arquivo deletado");
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
            track[ctx.Guild.Id] = track[ctx.Guild.Id].Shuffle();
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
            List<string> ids = track[ctx.Guild.Id].Take(5).ToList();
            List<IVideo> videos = youtubeService.GetVideosByList(ids);

            DiscordEmbedBuilder embed = new()
            {
                Title = "Fila de Musicas",
                Color = DiscordColor.Green,
            };
            int index = 1;
            foreach (IVideo video in videos)
            {
                embed.Description += $"{index} - {Formatter.MaskedUrl(video.Title, new Uri(video.Url))}\n";
                index++;
            }
            await ctx.Channel.SendMessageAsync(embed);
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
            MusicSkipped(this, ctx.Guild.Id);
            await ctx.CreateResponseAsync("Musica pulada (eu espero) ");
        }
    }
}