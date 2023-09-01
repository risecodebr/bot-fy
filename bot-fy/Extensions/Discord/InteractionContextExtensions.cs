using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace bot_fy.Extensions.Discord
{
    public static class InteractionContextExtensions
    {
        public static async Task<bool> ValidateChannels(this InteractionContext ctx)
        {
            if (ctx.Member?.VoiceState?.Channel == null)
            {
                await ctx.CreateResponseAsync("Você precisa estar em um canal de voz");
                return false;
            }

            if (ctx.Guild.CurrentMember.VoiceState?.Channel != null && ctx.Guild.CurrentMember.VoiceState?.Channel != ctx.Member?.VoiceState?.Channel)
            {
                await ctx.CreateResponseAsync("Você precisa estar no mesmo canal de voz que eu");
                return false;
            }

            DiscordChannel channel = ctx.Member.VoiceState?.Channel!;

            Permissions permissoes = channel.PermissionsFor(ctx.Guild.CurrentMember);

            if (!permissoes.HasPermission(Permissions.AccessChannels))
            {
                await ctx.CreateResponseAsync("Não tenho permissão para entrar no canal de voz");
                return false;
            }


            if (!permissoes.HasPermission(Permissions.UseVoice))
            {
                await ctx.CreateResponseAsync("Não tenho permissão para falar no canal de voz");
                return false;
            }

            return true;
        }
    }
}
