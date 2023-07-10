using Serilog;
using Serilog.Core;

namespace bot_fy.Utils
{
    public static class LoggerExtensions
    {
        public static Logger ConfigureLogger(this LoggerConfiguration logger)
        {
            return logger
                .WriteTo
                .Console(outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss.ff}][{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}