using BotFy.Commands;
using BotFy.Events;
using BotFy.Helpers;
using BotFy.Utils;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BotFy
{
    public class Program
    {
        public static async Task Main()
        {
            var token = EnvironmentHelper.Get("DISCORD_TOKEN");
            var guildId = EnvironmentHelper.Get<ulong>("GUILD_ID", 0);

            DiscordClientBuilder builder = DiscordClientBuilder
                .CreateDefault(token, DiscordIntents.AllUnprivileged)
                .UseInteractivity()
                .UseVoiceNext(new VoiceNextConfiguration())
                .UseCommands(extension =>
                {
                    extension.AddCommands([typeof(MusicCommand)], guildId);
                    var slashCommandProcessor = new SlashCommandProcessor(new()
                    {
                        RegisterCommands = true,
                    });
                    extension.AddProcessor(slashCommandProcessor);
                    extension.CommandErrored += OnCommandErrored.HandleEventAsync;
                    extension.CommandExecuted += OnCommandExecuted.HandleEventAsync;

                }, new CommandsConfiguration()
                {
                    DebugGuildId = guildId
                })
                .ConfigureEventHandlers(p =>
                {
                    p.AddEventHandlers<OnSessionCreated>();
                    p.AddEventHandlers<OnGuildDownloadCompleted>();
                    p.AddEventHandlers<OnComponentInteractionCreated>();
                })
                .SetLogLevel(LogLevel.Trace)
                .ConfigureServices(services =>
                {
                    var logger = new LoggerConfiguration().ConfigureLogger();

                    services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
                });

            await builder.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}