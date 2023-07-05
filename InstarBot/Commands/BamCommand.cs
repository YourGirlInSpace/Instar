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
    
    [SlashCommand("bam", "With this fun command you can bam someone on the head with the biggest hammer!")]
    public override async Task DoCommand([Summary("user", "The user you want to bam.")] IUser? user)
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