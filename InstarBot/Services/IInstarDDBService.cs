using System.Diagnostics.CodeAnalysis;

namespace PaxAndromeda.Instar.Services;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IInstarDDBService
{
    Task<bool> UpdateUserBirthday(Snowflake snowflake, DateTime birthday);
    Task<bool> UpdateUserJoinDate(Snowflake snowflake, DateTime joinDate);
    Task<bool> UpdateUserMembership(Snowflake snowflake, bool membershipGranted);
    Task<DateTimeOffset?> GetUserBirthday(Snowflake snowflake);
    Task<DateTimeOffset?> GetUserJoinDate(Snowflake snowflake);
    Task<bool?> GetUserMembership(Snowflake snowflake);
}