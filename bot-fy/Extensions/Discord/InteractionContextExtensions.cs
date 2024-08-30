using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace BotFy.Extensions.Discord
{
    public static class InteractionContextExtensions
    {
        public static async Task<bool> ValidateChannels(this CommandContext ctx)
        {
            if (ctx.Member?.VoiceState?.Channel == null)
            {
                await ctx.RespondAsync("Você precisa estar em um canal de voz");
                return false;
            }

            if (ctx.Guild.CurrentMember.VoiceState?.Channel != null && ctx.Guild.CurrentMember.VoiceState?.Channel != ctx.Member?.VoiceState?.Channel)
            {
                await ctx.RespondAsync("Você precisa estar no mesmo canal de voz que eu");
                return false;
            }

            DiscordChannel channel = ctx.Member.VoiceState?.Channel!;

            DiscordPermissions permissoes = channel.PermissionsFor(ctx.Guild.CurrentMember);

            if (!permissoes.HasPermission(DiscordPermissions.AccessChannels))
            {
                await ctx.RespondAsync("Não tenho permissão para entrar no canal de voz");
                return false;
            }


            if (!permissoes.HasPermission(DiscordPermissions.UseVoice))
            {
                await ctx.RespondAsync("Não tenho permissão para falar no canal de voz");
                return false;
            }

            return true;
        }
    }
}
