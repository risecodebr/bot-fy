using bot_fy.Discord;
using bot_fy.Extensions.Discord;
using bot_fy.Service;
using bot_fy.Utils;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Serilog;

namespace bot_fy
{
    public class Program
    {
        public static DiscordClient? Discord { get; private set; }

        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration().ConfigureLogger();
            await BotService.CreatePathsAsync();

            DiscordConfiguration cfg = new()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!,
                ReconnectIndefinitely = true,
                MinimumLogLevel = LogLevel.Error,
            };

            Discord = new DiscordClient(cfg);
            Discord.SessionCreated += Events.OnSessionCreated;
            Discord.GuildDownloadCompleted += Events.OnGuildDownloadCompleted;
            Discord.ComponentInteractionCreated += Events.OnComponentInteractionCreated;
            VoiceNextExtension vnext = Discord.UseVoiceNext();
            SlashCommandsExtension slash = Discord.UseSlashCommands();
            slash.SlashCommandErrored += EventsSlash.OnSlashCommandErrored;
            InteractivityExtension interactivity = Discord.UseInteractivity();
            slash.RegisterCommands();
            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}