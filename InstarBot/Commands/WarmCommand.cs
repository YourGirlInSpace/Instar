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
    
    [SlashCommand("warm", "Description")]
    public override async Task DoCommand([Summary("user", "The user you want to warm.")] IUser? user)
    {
        if (user == null)
        {
            await RespondAsync(GetRandomNoMentionPhrase());
        }
        else
        {
            await RespondAsync(string.Format(GetRandomPhrase(), user.Id));
        }
    }
}