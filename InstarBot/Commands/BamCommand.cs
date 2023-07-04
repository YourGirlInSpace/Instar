using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace PaxAndromeda.Instar.Commands;

[UsedImplicitly]
public class BamCommand : RandomPhraseCommand
{
    public BamCommand(IConfiguration config)
        : base(config, "Bam")
    { }
    
    [SlashCommand("bam", "Description")]
    public override async Task DoCommand([Summary("user", "The user you want to bam.")] IUser user)
    {
        await RespondAsync(string.Format(GetRandomPhrase(), user.Id));
    }
}