using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Diagnostics;

namespace bot_fy.Commands
{
    public class InfoCommand : ApplicationCommandModule
    {
        [SlashCommand("Info", "Receba informações tecnicas do bot fy")]
        public async Task Info(InteractionContext ctx)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Informações do bot fy",
                Color = DiscordColor.CornflowerBlue,
                Timestamp = DateTime.Now,
            };

            embed.AddField("**Tecnica**",
                $"> Online há: {Formatter.Timestamp(Process.GetCurrentProcess().StartTime, TimestampFormat.ShortTime)}\n" +
                $"> Desenvolvido por <@944942359169363989> e <@336211359106727936> ");

            embed.AddField("**Discord**",
                $"> Em `{ctx.Client.Guilds.Count}` servidores.\n" +
                $"> Usado por `{ctx.Client.Guilds.Sum(p => p.Value.MemberCount)}` usuários.\n");

            embed.AddField("**Sistema**",
                $"> Versão do Dotnet: `{Environment.Version}`\n" +
                $"> Versão do Ubuntu: `{Environment.OSVersion}`\n" +
                $"> Versão do DSharpPlus: `{ctx.Client.VersionString}`\n" +
                $"> Espaço disponivel em Disco: `{DriveInfo.GetDrives().Sum(p => p.AvailableFreeSpace / 1024 / 1024 / 1024)}gb`\n" +
                $"> Espaço total do Disco: `{DriveInfo.GetDrives().Sum(p => p.TotalSize / 1024 / 1024 / 1024)}gb`\n" +
                $"> Uso de memória da aplicação: `{Process.GetCurrentProcess().WorkingSet64 / 1024}mb`\n" +
                $"> Uso de memória total: `{Process.GetProcesses().Sum(p => p.WorkingSet64) / 1024}mb`\n", true);

            embed.AddField("**Ping**", $"> Ping API: `{ctx.Client.Ping}ms`");
            await ctx.CreateResponseAsync(embed);
        }
    }
}
