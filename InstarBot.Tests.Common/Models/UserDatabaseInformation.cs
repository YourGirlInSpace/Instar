using PaxAndromeda.Instar;

namespace InstarBot.Tests.Models;

public sealed record UserDatabaseInformation(Snowflake Snowflake)
{
    public DateTime Birthday { get; set; }
    public DateTime JoinDate { get; set; }
    public bool GrantedMembership { get; set; }
}