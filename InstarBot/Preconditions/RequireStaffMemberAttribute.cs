using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace PaxAndromeda.Instar.Preconditions;

public sealed class RequireStaffMemberAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
        ICommandInfo commandInfo, IServiceProvider services)
    {
        var config = services.GetService<IConfiguration>();

        var authorizedStaffRoles = config?.GetSection("AuthorizedStaffID").Get<List<ulong>>();
        if (authorizedStaffRoles is null)
        {
            Log.Error("Authorized staff roles are not set in config!");
            return Task.FromResult(PreconditionResult.FromError("Unable to determine eligibility to run this command"));
        }

        if (context.User is not IGuildUser guildUser)
        {
            Log.Warning("Context user is not IGuildUser!");
            return Task.FromResult(PreconditionResult.FromError("Unable to determine eligibility to run this command"));
        }

        var intersection = guildUser.RoleIds.Intersect(authorizedStaffRoles);

        return Task.FromResult(intersection.Any()
            ? PreconditionResult.FromSuccess()
            : PreconditionResult.FromError("You are not eligible to run this command"));
    }
}