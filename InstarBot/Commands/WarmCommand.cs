using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace PaxAndromeda.Instar.Commands;

[UsedImplicitly]
public class WarmCommand : RandomPhraseCommand
{
    public WarmCommand(IConfiguration config)
        : base(config, "Warm")
    { }
    
    [SlashCommand("warm", "With this fun command you can warm someone up!")]
    public override async Task DoCommand([Summary("user", "The user you want to warm.")] IUser? user)
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