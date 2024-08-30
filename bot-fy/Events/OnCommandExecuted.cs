using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using Serilog;

namespace BotFy.Events;

public class OnCommandExecuted
{
    public static async Task HandleEventAsync(CommandsExtension sender, CommandExecutedEventArgs args)
    {
        var content = $"""
            User {args.Context.User.Username} 
            executed command {args.Context.Command.Name}
            with parameters {string.Join(", ", args.Context.Arguments.Select(x => $"{x.Key.Name} - {x.Value}"))}
            """;
        Log.Debug(content);
    }
}
