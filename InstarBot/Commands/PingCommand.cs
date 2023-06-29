using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar.Commands;

// Required to be unsealed for mocking
[SuppressMessage("ReSharper", "ClassCanBeSealed.Global")]
public class PingCommand : BaseCommand
{
    [UsedImplicitly]
    [RequireOwner]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("ping", "Provides a way to test whether Instar is online.")]
    public async Task Ping()
    {
        await RespondAsync("Pong!", ephemeral: true);
    }
}