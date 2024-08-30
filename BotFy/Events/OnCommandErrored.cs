using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Entities;
using System.Text;

namespace BotFy.Events;

public class OnCommandErrored
{
    public static async Task HandleEventAsync(CommandsExtension sender, CommandErroredEventArgs args)
    {
        DiscordEmbedBuilder embed = new()
        {
            Title = "Erro",
            Color = DiscordColor.Red,
            Timestamp = DateTime.UtcNow,
        };

        StringBuilder strings = new();
        strings.AppendLine($"Servidor: {args.Context.Guild.Name} - ({args.Context.Guild.Id})");
        strings.AppendLine($"Usuário: {args.Context.User.Mention} - ({args.Context.User.Id})");
        strings.AppendLine($"Comando: {args.Context?.Command?.Name}");
        strings.AppendLine($"Erro: {args.Exception.Message}\n");
        strings.AppendLine($"Stack: ```{args.Exception.StackTrace}```");
        embed.WithDescription(strings.ToString());

        DiscordChannel channel_error = await sender.Client.GetChannelAsync(1145407009697583134);
        await channel_error.SendMessageAsync(embed);

        await args.Context.Channel.SendMessageAsync("Ocorreu um erro, esse erro foi reportado para o desenvolvedor.");
    }
}
