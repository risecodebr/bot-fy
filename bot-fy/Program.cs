using bot_fy.Discord;
using bot_fy.Extensions.Discord;
using bot_fy.Service;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YoutubeExplode;

namespace bot_fy;

public class Program
{
    public static DiscordClient? Discord { get; private set; }

    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        await BotService.CreatePathsAsync();

        DiscordConfiguration cfg = new()
        {
            Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!,
            ReconnectIndefinitely = true,
            MinimumLogLevel = LogLevel.Debug,
        };

        Discord = new DiscordClient(cfg);
        Discord.SessionCreated += Events.OnSessionCreated;
        Discord.GuildDownloadCompleted += Events.OnGuildDownloadCompleted;
        Discord.ComponentInteractionCreated += Events.OnComponentInteractionCreated;

        VoiceNextExtension vnext = Discord.UseVoiceNext();

        var endpoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1", // From your server configuration.
            Port = 2333 // From your server configuration
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = "youshallnotpass", // From your server configuration.
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };

        var lavalink = Discord.UseLavalink();

        IServiceCollection services = new ServiceCollection();
        services.AddScoped<AudioService>();
        services.AddScoped<BotService>();
        services.AddScoped<YoutubeClient>();
        services.AddScoped<YoutubeService>();

        SlashCommandsExtension slash = Discord.UseSlashCommands(new SlashCommandsConfiguration()
        {
            Services = services.BuildServiceProvider(),
        });

        slash.SlashCommandErrored += EventsSlash.OnSlashCommandErrored;
        InteractivityExtension interactivity = Discord.UseInteractivity();
        slash.RegisterCommands();
        await Discord.ConnectAsync();
        await lavalink.ConnectAsync(lavalinkConfig);
        await Task.Delay(-1);
    }
}