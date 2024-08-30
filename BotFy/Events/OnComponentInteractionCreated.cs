using BotFy.Commands;
using BotFy.Extensions.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using YoutubeExplode.Videos;

namespace BotFy.Events;

public class OnComponentInteractionCreated : IEventHandler<ComponentInteractionCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs args)
    {
        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        if (args.Id == "skip")
        {
            MusicCommand.Skip(args.Guild.Id);
        }

        else if (args.Id == "stop")
        {
            MusicCommand.Stop(args.Guild.Id);
        }

        else if (args.Id == "shuffle")
        {
            MusicCommand.Shuffle(args.Guild.Id);
        }

        else if (args.Id == "queue")
        {
            IEnumerable<IVideo> queue = MusicCommand.GetQueue(args.Guild.Id);

            await args.Channel.SendPaginatedMusicsAsync(args.User, queue);
        }


    }
}
