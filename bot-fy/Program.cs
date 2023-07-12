using bot_fy.Commands;
using bot_fy.Discord;
using bot_fy.Service;
using bot_fy.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
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
            await BotService.CreatePathsAsync();

            Log.Logger = new LoggerConfiguration().ConfigureLogger();

            DiscordConfiguration cfg = new()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!,
                ReconnectIndefinitely = true,
                MinimumLogLevel = LogLevel.Debug,
            };

            Discord = new DiscordClient(cfg);
            Discord.SessionCreated += Events.OnSessionCreated;
            Discord.GuildDownloadCompleted += Events.OnGuildDownloadCompleted;
            VoiceNextExtension vnext = Discord.UseVoiceNext();
            SlashCommandsExtension slash = Discord.UseSlashCommands();
            slash.RegisterCommands<MusicCommand>(880904935787601960);
            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}