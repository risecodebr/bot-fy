using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace BotFy.Events;

public class OnSessionCreated : IEventHandler<SessionCreatedEventArgs>
{
    public Task HandleEventAsync(DiscordClient sender, SessionCreatedEventArgs eventArgs)
    {
        Log.Information($"Bot conectado como {sender.CurrentUser.Username}#{sender.CurrentUser.Discriminator}");
        return Task.CompletedTask;
    }
}
