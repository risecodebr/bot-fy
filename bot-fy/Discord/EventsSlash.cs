using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Text;

namespace bot_fy.Discord
{
    public class EventsSlash
    {
        public static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Erro",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
            };

            StringBuilder strings = new();
            strings.AppendLine($"Servidor: {args.Context.Guild.Name} - ({args.Context.Guild.Id})");
            strings.AppendLine($"Usuário: {args.Context.User.Mention} - ({args.Context.User.Id})");
            strings.AppendLine($"Comando: {args.Context.CommandName}");
            strings.AppendLine($"Erro: {args.Exception.Message}\n");
            strings.AppendLine($"Stack: ```{args.Exception.StackTrace}```");
            embed.WithDescription(strings.ToString());

            DiscordChannel channel_error = await sender.Client.GetChannelAsync(1145407009697583134);
            await channel_error.SendMessageAsync(embed);

            await args.Context.Channel.SendMessageAsync("Ocorreu um erro, esse erro foi reportado para o desenvolvedor.");
        }
    }
}
