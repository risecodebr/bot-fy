using DSharpPlus.SlashCommands;

namespace bot_fy.Discord.Extensions
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

            return true;
        }
    }
}
