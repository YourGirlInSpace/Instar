using System.Configuration;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar.ConfigModels;

public sealed class Team
{
    public string InternalID { get; [UsedImplicitly] set; } = null!;
    public string Name { get; [UsedImplicitly] set; } = null!;

    // ReSharper disable once InconsistentNaming
    [ConfigurationProperty("ID", DefaultValue = "999")]
    [SnowflakeType(SnowflakeType.Role)]
    public Snowflake ID { get; [UsedImplicitly] set; } = null!;

    // ReSharper disable once IdentifierTypo
    [ConfigurationProperty("Teamleader", DefaultValue = "999")]
    [SnowflakeType(SnowflakeType.User)]
    public Snowflake Teamleader { get; [UsedImplicitly] set; } = null!;

    public uint Color { get; [UsedImplicitly] set; }
    public int Priority { get; [UsedImplicitly] set; }
}