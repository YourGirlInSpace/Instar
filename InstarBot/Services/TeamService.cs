using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Discord;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PaxAndromeda.Instar.Services;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class TeamService
{
    private readonly Dictionary<string, Team> _teams;
    private readonly Dictionary<Snowflake, string> _teamIdRefMap;

    public TeamService(IConfiguration config)
    {
        var teamList = config.GetSection("Teams").Get<List<Team>>();
        Guard.Against.Null(teamList);


        var teamsConfig = teamList.ToDictionary(n => n.InternalID, n => n);
        var teamIdRefMap = teamList.ToDictionary(n => n.ID, n => n.InternalID);

        if (teamsConfig is null || teamIdRefMap is null)
            throw new ConfigurationException(
                "Instar configuration doesn't appear to have teams configured, or the configuration was loaded incorrectly.");

        _teams = teamsConfig;
        _teamIdRefMap = teamIdRefMap;
    }

    public bool Exists(string teamRef)
    {
        return _teams.ContainsKey(teamRef);
    }

    public bool Exists(Snowflake snowflake)
    {
        return _teamIdRefMap.ContainsKey(snowflake);
    }

    public Team Get(string teamRef)
    {
        return _teams[teamRef];
    }

    public Team Get(Snowflake snowflake)
    {
        return Get(_teamIdRefMap[snowflake]);
    }

    public IEnumerable<Team> GetTeams(PageTarget pageTarget)
    {
        var teamRefs = pageTarget.GetAttributesOfType<TeamRefAttribute>()?.Select(n => n.InternalID) ??
                       new List<string>();

        foreach (var internalId in teamRefs)
        {
            if (!_teams.ContainsKey(internalId))
                throw new InvalidStateException("Failed to find team with internal ID " + internalId);

            yield return _teams[internalId];
        }
    }

    public string GetTeamLeadMention(PageTarget pageTarget)
    {
        return string.Join(' ', GetTeams(pageTarget).Select(n => Snowflake.GetMention(() => n.Teamleader)));
    }

    public string GetTeamMention(PageTarget pageTarget)
    {
        return string.Join(' ', GetTeams(pageTarget).Select(n => Snowflake.GetMention(() => n.ID)));
    }

    /// <summary>
    ///     Determines the <paramref name="user" />'s highest staff team, if they are staff.
    /// </summary>
    /// <param name="user">The user in question</param>
    /// <returns>The user's highest staff team, or null if the user is not staff.</returns>
    public Team? GetUserPrimaryStaffTeam(IGuildUser user)
    {
        Team? highestTeam = null;
        Log.Debug("User roles: {Roles}", string.Join(", ", user.RoleIds));
        foreach (var role in user.RoleIds.Select(n => new Snowflake(n)))
        {
            if (!Exists(role))
                continue;

            var st = Get(role);

            Log.Debug("Team role found: {Role} with internal ID {InternalID}", role, st.InternalID);

            // Set the team if it is null
            highestTeam ??= st;
            if (st.Priority < highestTeam.Priority)
                highestTeam = st;
        }

        if (highestTeam is not null)
            Log.Debug("Highest team: {TeamID} {TeamName}", highestTeam.ID, highestTeam.Name);
        else Log.Debug("Highest team was not found");

        return highestTeam;
    }
}