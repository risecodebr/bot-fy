using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Text;

namespace bot_fy.Discord
{
    public class EventsSlash
    {
        public static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Erro",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
            };

            StringBuilder strings = new();
            strings.AppendLine($"Servidor: {e.Context.Guild.Name} - ({e.Context.Guild.Id})");
            strings.AppendLine($"Usuário: {e.Context.User.Mention} - ({e.Context.User.Id})");
            strings.AppendLine($"Comando: {e.Context.CommandName}");
            strings.AppendLine($"Erro: {e.Exception.Message}\n");
            strings.AppendLine($"Arquivo: {e.Exception.Source} ({e.Exception.HResult})")
            strings.AppendLine($"Stack: ```{e.Exception.StackTrace}```");
            embed.WithDescription(strings.ToString());

            DiscordChannel channel_error = await sender.Client.GetChannelAsync(1145407009697583134);
            await channel_error.SendMessageAsync(embed);
        }
    }
}
