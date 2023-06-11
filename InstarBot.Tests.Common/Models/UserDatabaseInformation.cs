using JetBrains.Annotations;
using PaxAndromeda.Instar;

namespace InstarBot.Tests.Models;

public record UserDatabaseInformation(Snowflake Snowflake)
{
    public DateTime Birthday { get; set; }
    public DateTime JoinDate { get; [UsedImplicitly] set; }
}