using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar.ConfigModels;

public sealed class AutoMemberConfig
{
    [SnowflakeType(SnowflakeType.Role)] public Snowflake HoldRole { get; set; } = null!;
    [SnowflakeType(SnowflakeType.Channel)] public Snowflake IntroductionChannel { get; set; } = null!;
    public int MinimumJoinAge { get; set; }
    public int MinimumMessages { get; set; }
    public int MinimumMessageTime { get; set; }
    public List<RoleGroup> RequiredRoles { get; set; } = null!;
    public bool EnableGaiusCheck { get; set; }
}

[UsedImplicitly]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class RoleGroup
{
    [UsedImplicitly] public string GroupName { get; set; } = null!;
    public List<Snowflake> Roles { get; set; } = null!;
}