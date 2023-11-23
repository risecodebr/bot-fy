using bot_fy.Extensions;
using bot_fy.Extensions.Discord;
using bot_fy.Service;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;
using YoutubeExplode.Videos;

namespace bot_fy.Commands;

public class MusicCommand(YoutubeService youtube) : ApplicationCommandModule
{
    private static readonly Dictionary<ulong, Queue<LavalinkTrack>> tracks = [];

    private static event EventHandler<ulong> OnMusicSkipped;
    private static event EventHandler<ulong> OnMusicStopped;

    [SlashCommand("play", "Reproduza sua musica ou playlist")]
    public async Task Play(InteractionContext ctx, [Option("link", "Link da musica, playlist ou mix do Youtube")] string termo)
    {
        var lava = ctx.Client.GetLavalink();

        var node = lava.ConnectedNodes.Values.First();

        if (ctx.Channel.Type != ChannelType.Voice)
        {
            await ctx.CreateResponseAsync("Not a valid voice ctx.Channel.");
            return;
        }

        await node.ConnectAsync(ctx.Channel);
        await ctx.CreateResponseAsync($"Joined {ctx.Channel.Name}!");

        tracks.TryAdd(ctx.Guild.Id, new Queue<LavalinkTrack>());

        var search = await youtube.GetResultsAsync(termo, ctx.Channel);

        await ctx.Channel.SendMessageAsync($"{search.Count} resultados");

        var video = search.First();

        await ctx.Channel.SendMessageAsync($"Adicionado {video.Title} a fila");
        var re = await node.Rest.GetTracksAsync(video.Title);
        Console.WriteLine(re.Tracks.Count());
        tracks[ctx.Guild.Id].Enqueue(re.Tracks.First());

        search.RemoveAt(0);

        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        conn.PlaybackFinished += async (sender, args) =>
        {
            if (args.Reason != TrackEndReason.Finished)
                return;

            if (tracks[ctx.Guild.Id].Count == 0)
            {
                await ctx.Channel.SendMessageAsync("Fila vazia");
                return;
            }

            await ctx.Channel.SendMessageAsync($"Tocando {tracks[ctx.Guild.Id].Peek().Title}");
            await conn.PlayAsync(tracks[ctx.Guild.Id].Dequeue());
        };

        var track = tracks[ctx.Guild.Id].Dequeue();
        await ctx.Channel.SendMessageAsync($"Tocando {track.Title}");
        await conn.PlayAsync(track);

        foreach (var videoooo in search)
        {
            await ctx.Channel.SendMessageAsync($"Adicionado {videoooo.Title} a fila");
            var reee = await node.Rest.GetTracksAsync(videoooo.Title);
            Console.WriteLine(re.Tracks.Count());
            tracks[ctx.Guild.Id].Enqueue(reee.Tracks.First());
        }

    }

    [SlashCommand("shuffle", "Deixa a fila de musicas aleatoria")]
    public async Task Shuffle(InteractionContext ctx)
    {
        if (!tracks.ContainsKey(ctx.Guild.Id))
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

        if (!tracks.ContainsKey(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync("Nenhuma musica na fila");
            return;
        }

        await ctx.Channel.SendPaginatedMusicsAsync(ctx.User, tracks[ctx.Guild.Id]);
    }

    [SlashCommand("clear", "Limpa a fila de musicas")]
    public async Task Clear(InteractionContext ctx)
    {
        if (!tracks.ContainsKey(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync("Nenhuma musica na fila");
            return;
        }
        tracks[ctx.Guild.Id].Clear();
        await ctx.CreateResponseAsync("Fila limpa");
    }

    [SlashCommand("skip", "Pule a musica atual")]
    public async Task Skip(InteractionContext ctx)
    {
        if (!tracks.ContainsKey(ctx.Guild.Id))
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
        tracks[guildId] = tracks[guildId].Shuffle();
    }
    public static IEnumerable<LavalinkTrack> GetQueue(ulong guildId)
    {
        return tracks[guildId];
    }
}