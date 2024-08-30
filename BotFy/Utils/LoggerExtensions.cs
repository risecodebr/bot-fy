using BotFy.Helpers;
using Serilog;

namespace BotFy.Utils
{
    public static class LoggerExtensions
    {
        public static ILogger ConfigureLogger(this LoggerConfiguration logger)
        {
            return logger
                .Enrich.With(new LoggerHelper())
                .WriteTo
                .Console(outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss.ff}][{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}