using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Discord;

namespace bot_fy.Utils
{
    public static class LoggerExtensions
    {
        public static Logger ConfigureLogger(this LoggerConfiguration logger)
        {
            return logger
                .WriteTo
                .Discord(LogEventLevel.Debug, config =>
                {
                    config.TimestampFormat = "dd/MM/yyyy HH:mm:ss.ff";
                    config.WebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_LOGS_URL");
                })
                .WriteTo
                .Console(outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss.ff}][{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}