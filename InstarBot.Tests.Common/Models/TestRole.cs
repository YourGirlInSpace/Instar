using Discord;
using PaxAndromeda.Instar;

#pragma warning disable CS8625

namespace InstarBot.Tests.Models;

public class TestRole : IRole
{
    internal TestRole(Snowflake snowflake)
    {
        Id = snowflake;
        CreatedAt = snowflake.Time;
    }

    public ulong Id { get; }
    public DateTimeOffset CreatedAt { get; }

    public Task DeleteAsync(RequestOptions options = null)
    {
        throw new NotImplementedException();
    }

    public string Mention { get; } = null!;

    public int CompareTo(IRole? other)
    {
        throw new NotImplementedException();
    }

    public Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null)
    {
        throw new NotImplementedException();
    }

    public string GetIconUrl()
    {
        throw new NotImplementedException();
    }

    public IGuild Guild { get; } = null!;
    public Color Color { get; } = default!;
    public bool IsHoisted { get; } = default!;
    public bool IsManaged { get; } = default!;
    public bool IsMentionable { get; } = default!;
    public string Name { get; } = null!;
    public string Icon { get; } = null!;
    public Emoji Emoji { get; } = null!;
    public GuildPermissions Permissions { get; } = default!;
    public int Position { get; } = default!;
    public RoleTags Tags { get; } = null!;
}