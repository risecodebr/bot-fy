using DSharpPlus;
using DSharpPlus.Entities;
using Serilog.Core;
using Serilog.Events;

namespace BotFy.Helpers;

public class LoggerHelper : ILogEventEnricher
{
    private static readonly string DISCORD_WEBHOOK_LOGS_URL = EnvironmentHelper.Get("DISCORD_WEBHOOK_LOGS_URL");

    private static readonly Dictionary<LogEventLevel, DiscordColor> colors = new()
    {
        { LogEventLevel.Debug, DiscordColor.Grayple },
        { LogEventLevel.Error, DiscordColor.Red },
        { LogEventLevel.Fatal, DiscordColor.Red },
        { LogEventLevel.Information, DiscordColor.Green },
        { LogEventLevel.Verbose, DiscordColor.Grayple },
        { LogEventLevel.Warning, DiscordColor.Yellow }
    };

    public async void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        DiscordWebhookClient webhook = new();
        await webhook.AddWebhookAsync(new Uri(DISCORD_WEBHOOK_LOGS_URL));

        var embed = new DiscordEmbedBuilder()
            .WithColor(colors[logEvent.Level])
            .WithDescription(logEvent.RenderMessage())
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("BotFy", null)
            .WithAuthor("BotFy", null, null);

        DiscordWebhookBuilder builder = new();
        builder.AddEmbed(embed);

        await webhook.BroadcastMessageAsync(builder);

    }
}
