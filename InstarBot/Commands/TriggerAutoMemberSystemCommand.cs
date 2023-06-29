using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using PaxAndromeda.Instar.Services;

namespace PaxAndromeda.Instar.Commands;

public sealed class TriggerAutoMemberSystemCommand : BaseCommand
{
    private readonly AutoMemberSystem _ams;

    public TriggerAutoMemberSystemCommand(AutoMemberSystem ams)
    {
        _ams = ams;
    }
    
    [UsedImplicitly]
    [RequireOwner]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("runams", "Manually triggers an auto member system run.")]
    public async Task RunAMS()
    {
        await RespondAsync("Auto Member System is running!", ephemeral: true);
        
        // Run it asynchronously
        await _ams.RunAsync();
    }
}