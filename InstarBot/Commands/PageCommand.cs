using System.Text;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PaxAndromeda.Instar.Commands;

public class PageCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Dictionary<ulong, Team> _teams;

    public PageCommand(IConfiguration config)
    {
        var teamsConfig =
            config.GetSection("Teams").Get<List<Team>>()?
                .ToDictionary(n => n.ID, n => n);

        _teams = teamsConfig ?? throw new InvalidStateException(
            "Instar.conf.json doesn't appear to have teams configured, or the configuration was loaded incorrectly.");
    }

    [UsedImplicitly]
    [SlashCommand("page", "This command initiates a directed page.")]
#if DEBUG
    [RequireRole(985521877122428978, Group = "Staff")] // Staff role
    [RequireRole(1113478706250395759, Group = "Staff")] // Community Manager role
#else
    [RequireRole(793607635608928257, Group = "Staff")] // Staff role
    [RequireRole(957411837920567356, Group = "Staff")] // Community Manager role
#endif
    [DefaultMemberPermissions(GuildPermission.MuteMembers)] // Stupid way to hide this command for unauthorized personnel
    public async Task Page(
        [Summary("team", "The team you wish to page.")]
        PageTarget team,

        [MinLength(12)]
        [Summary("reason", "The reason for the page.")]
        string reason,

        [Summary("teamlead", "Do you wish to page the team lead for the team you selected?")]
        bool teamLead = false,

        [Summary("message", "A message link related to the reason you're paging.")]
        string message = "",

        [Summary("user", "The user you are paging about.")]
        IUser? user = null,

        [Summary("channel", "The channel you are paging about.")]
        IChannel? channel = null)
    {
        try
        {
            Log.Verbose("User {User} is attempting to page {Team}: {Reason}", Context.User.Id, team, reason);
            
            if (Context.User is not IGuildUser guildUser)
                throw new InvalidStateException("Context.User was not an IGuildUser");

            Team? userTeam = GetUserPrimaryStaffTeam(guildUser);
            if (!CheckPermissions(guildUser, userTeam, team, teamLead, out string? response))
            {
                await RespondAsync(response, ephemeral: true);
                return;
            }

            string mention;
            if (team == PageTarget.Test)
                mention = "This is a __**TEST**__ page.";
            else if (teamLead)
                mention = GetTeamLeadMention(team);
            else
                mention = GetTeamMention(team);


            await RespondAsync(
                text: mention,
                embed: BuildEmbed(reason, message, user, channel, userTeam!, guildUser),
                allowedMentions: AllowedMentions.All);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send page from {User}", Context.User.Id);
            await RespondAsync("Failed to process command due to an internal server error.", ephemeral: true);
        }
    }

    private string GetTeamLeadMention(PageTarget pageTarget)
    {
        StringBuilder pingBuilder = new();
        foreach (ulong teamId in pageTarget.GetTeamIDs())
        {
            if (!_teams.ContainsKey(teamId))
                throw new InvalidStateException(
                    "Failed to determine page targets due to misconfigured PageTarget enum for team ID " + teamId);

            Team team = _teams[teamId];
            pingBuilder.Append($"<@{team.Teamleader}> ");
        }

        string target = pingBuilder.ToString();

        return target.Length == 0 ? target : target[..^1];
    }

    private static string GetTeamMention(PageTarget pageTarget)
    {
        StringBuilder pingBuilder = new();
        foreach (ulong teamId in pageTarget.GetTeamIDs())
            pingBuilder.Append($"<@&{teamId}> ");

        string target = pingBuilder.ToString();

        return target.Length == 0 ? target : target[..^1];
    }

    /// <summary>
    /// Determine's the <paramref name="user"/>'s highest staff team, if they are staff.
    /// </summary>
    /// <param name="user">The user in question</param>
    /// <returns>The user's highest staff team, or null if the user is not staff.</returns>
    private Team? GetUserPrimaryStaffTeam(IGuildUser user)
    {
        Team? highestTeam = null;
        foreach (ulong roleId in user.RoleIds)
        {
            if (!_teams.ContainsKey(roleId))
                continue;

            Team st = _teams[roleId];

            // Set the team if it is null
            highestTeam ??= st;
            if (st.Priority < highestTeam.Priority)
                highestTeam = st;
        }

        return highestTeam;
    }

    /// <summary>
    /// Determines whether a <paramref name="user"/> has the authority to issue a page to <paramref name="pageTarget"/>.
    /// </summary>
    /// <param name="user">The user attempting to issue a page</param>
    /// <param name="team">The user's staff team</param>
    /// <param name="pageTarget">The target the user is attempting to page</param>
    /// <param name="teamLead">Whether the user is attempting to page the team's team leader</param>
    /// <param name="response">A response string to show to the user if they are not authorized to send a page.</param>
    /// <returns>A boolean indicating whether the user has permissions to send this page.</returns>
    private static bool CheckPermissions(IGuildUser user, Team? team, PageTarget pageTarget, bool teamLead, out string? response)
    {
        response = null;

        if (team is null)
        {
            response = "You are not authorized to use this command.";
            Log.Information("{User} was not authorized to send a page.", user.Id);
            return false;
        }

        if (pageTarget == PageTarget.Test)
            return true;

            // Check permissions.  Only mod+ can send an "all" page
        if (team.Priority > 3 && pageTarget == PageTarget.All) // i.e. Helper, Community Manager
        {
            response = "You are not authorized to send a page to the entire staff team.";
            Log.Information("{User} was not authorized to send a page to the entire staff team.", user.Id);
            return false;
        }

        if (pageTarget == PageTarget.All && teamLead)
        {
            response = "Failed to send page.  The 'All' team does not have a teamleader.  If intended to page the owner, please select the Owner as the team.";
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Builds the page embed.
    /// </summary>
    /// <param name="reason">The reason for the page.</param>
    /// <param name="message">A message link.  May be null.</param>
    /// <param name="targetUser">The user being paged about.  May be null.</param>
    /// <param name="channel">The channel being paged about.  May be null.</param>
    /// <param name="userTeam">The paging user's team</param>
    /// <param name="pagingUser">The paging user</param>
    /// <returns>A standard embed embodying all parameters provided</returns>
    private static Embed BuildEmbed(
        string reason,
        string message, 
        IUser? targetUser,
        IChannel? channel, 
        Team userTeam,
        IGuildUser pagingUser)
    {
        var fields = new List<EmbedFieldBuilder>();

        if (targetUser is not null)
            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("User").WithValue($"<@{targetUser.Id}>"));

        if (channel is not null)
            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Channel").WithValue($"<#{channel.Id}>"));

        if (!string.IsNullOrEmpty(message))
            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Message").WithValue(message));
        
        EmbedBuilder builder = new EmbedBuilder()
            // Set up all the basic stuff first
            .WithCurrentTimestamp()
            .WithColor(userTeam.Color)
            .WithAuthor(pagingUser.Nickname ?? pagingUser.Username, pagingUser.GetAvatarUrl())
            .WithFooter(new EmbedFooterBuilder()
                .WithText("Instar Paging System")
                .WithIconUrl("https://spacegirl.s3.us-east-1.amazonaws.com/instar.png"))
            // Description
            .WithDescription($"```{reason}```")
            .WithFields(fields);

        Embed embed = builder.Build();
        return embed;
    }
}