using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace PaxAndromeda.Instar.Commands;

[UsedImplicitly]
public class LickCommand : RandomPhraseCommand
{
    public LickCommand(IConfiguration config)
        : base(config, "Lick")
    { }
    
    [SlashCommand("lick", "With this fun command you can lick someone?!")]
    public override async Task DoCommand([Summary("user", "The user you want to lick.")] IUser? user)
    {
        if (user is null)
        {
            await RespondAsync(GetRandomNoMentionPhrase());
        }
        else
        {
            await RespondAsync(string.Format(GetRandomPhrase(), user.Id));
        }
    }
}