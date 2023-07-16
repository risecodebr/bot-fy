using bot_fy.Commands;
using DSharpPlus.SlashCommands;
using Serilog;

namespace bot_fy.Discord.Extensions
{
    public static class SlashCommandsExtensionExtensions
    {
        public static void RegisterCommands(this SlashCommandsExtension slash)
        {
            string? guild_id_enviroment = Environment.GetEnvironmentVariable("GUILD_ID");
            ulong? guild_id = null;
            if (guild_id_enviroment is not null)
            {
                guild_id = ulong.Parse(guild_id_enviroment);
            }
            slash.RegisterCommands<MusicCommand>(guild_id);

            Log.Information($"Registered commands {((guild_id == null) ? "" : "in guild " + guild_id)}");
        }
    }
}
