using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace BotFy.Events;

public class OnGuildDownloadCompleted : IEventHandler<GuildDownloadCompletedEventArgs>
{
    public Task HandleEventAsync(DiscordClient sender, GuildDownloadCompletedEventArgs args)
    {
        Log.Information($"Carregado {args.Guilds.Count} servidores");
        Log.Information($"Carregado {args.Guilds.Sum(g => g.Value.MemberCount)} membros");
        Log.Information($"Carregado {args.Guilds.Sum(g => g.Value.Channels.Count)} canais");

        return Task.CompletedTask;
    }
}
