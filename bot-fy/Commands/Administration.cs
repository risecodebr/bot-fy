using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;

namespace bot_fy.Commands
{
    public class Administration : ApplicationCommandModule
    {
        [SlashCommand("servers", "Listar todos os servidores")]
        [RequireUserId(944942359169363989, 336211359106727936)]
        public async Task Servers(InteractionContext ctx)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "Servidores",
                Color = DiscordColor.Green,
                Timestamp = DateTime.Now,
            };
            StringBuilder strings = new();
            foreach (DiscordGuild guild in ctx.Client.Guilds.Values.OrderByDescending(p => p.MemberCount))
            {
                strings.AppendLine($"{guild.Name} - ({guild.Id}) - {guild.MemberCount} membros - {guild.CurrentMember.JoinedAt:dd:MM:yyyy}");
            }
            embed.WithDescription(strings.ToString());

            await ctx.CreateResponseAsync(embed);
        }
    }

    public class RequireUserIdAttribute : SlashCheckBaseAttribute
    {
        public ulong[] usersIds;

        public RequireUserIdAttribute(params ulong[] usersIds)
        {
            this.usersIds = usersIds;
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.FromResult(usersIds.Contains(ctx.User.Id));
        }
    }
}
