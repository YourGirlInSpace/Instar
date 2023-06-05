namespace PaxAndromeda.Instar;

public static class PageTargetExtensions
{
    public static IEnumerable<ulong> GetTeamIDs(this PageTarget target)
    {
        var teamAttrs = target.GetAttributesOfType<TeamIDAttribute>();
        if (teamAttrs is null)
            throw new InvalidStateException("Failed to determine page targets due to misconfigured PageTarget enum");

        return teamAttrs.Select(n => n.ID);
    }
}