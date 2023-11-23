using bot_fy.Commands;
using bot_fy.Extensions.Discord;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Serilog;
using YoutubeExplode.Videos;

namespace bot_fy.Discord;

public class Events
{
    public static Task OnSessionCreated(DiscordClient client, SessionReadyEventArgs args)
    {
        Log.Information($"Bot conectado como {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
        return Task.CompletedTask;
    }

    public static Task OnGuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs args)
    {
        Log.Information($"Carregado {args.Guilds.Count} servidores");
        Log.Information($"Carregado {args.Guilds.Sum(g => g.Value.MemberCount)} membros");
        Log.Information($"Carregado {args.Guilds.Sum(g => g.Value.Channels.Count)} canais");
        return Task.CompletedTask;
    }

    public static async Task OnComponentInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

        if (args.Id == "skip")
        {
            MusicCommand.Skip(args.Guild.Id);
        }

        else if (args.Id == "stop")
        {
            MusicCommand.Stop(args.Guild.Id);
        }

        else if (args.Id == "shuffle")
        {
            MusicCommand.Shuffle(args.Guild.Id);
        }

        else if (args.Id == "queue")
        {
            IEnumerable<LavalinkTrack> queue = MusicCommand.GetQueue(args.Guild.Id);

            await args.Channel.SendPaginatedMusicsAsync(args.User, queue);
        }
    }
}