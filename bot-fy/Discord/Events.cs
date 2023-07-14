using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace bot_fy.Discord
{
    public class Events
    {
        public static Task OnSessionCreated(DiscordClient client, SessionReadyEventArgs e)
        {
            Log.Information($"Bot conectado como {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
            return Task.CompletedTask;
        }

        public static Task OnGuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs e)
        {
            Log.Information($"Carregado {e.Guilds.Count} servidores");
            return Task.CompletedTask;
        }
    }
}