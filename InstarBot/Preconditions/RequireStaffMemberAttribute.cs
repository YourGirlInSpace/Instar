using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using PaxAndromeda.Instar.Services;
using Serilog;

namespace PaxAndromeda.Instar.Preconditions;

public sealed class RequireStaffMemberAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
        ICommandInfo commandInfo, IServiceProvider services)
    {
        var dynamicConfig = services.GetService<IDynamicConfigService>();

        if (dynamicConfig is null)
        {
            Log.Error("Failed to determine command requirements due to configuration mishap");
            return PreconditionResult.FromError("Unable to determine eligibility to run this command.");
        }
        
        var cfg = await dynamicConfig.GetConfig();

        var authorizedStaffRoles = cfg.AuthorizedStaffID.Select(n => n.ID);

        if (context.User is not IGuildUser guildUser)
        {
            Log.Warning("Context user is not IGuildUser!");
            return PreconditionResult.FromError("Unable to determine eligibility to run this command");
        }

        var intersection = guildUser.RoleIds.Intersect(authorizedStaffRoles);

        return intersection.Any()
            ? PreconditionResult.FromSuccess()
            : PreconditionResult.FromError("You are not eligible to run this command");
    }
}