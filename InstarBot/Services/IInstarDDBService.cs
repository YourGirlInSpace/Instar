namespace PaxAndromeda.Instar.Services;

public interface IInstarDDBService
{
    Task<bool> UpdateUserBirthday(Snowflake snowflake, DateTime birthday);

    // This method will be used shortly...
    // ReSharper disable once UnusedMember.Global
    Task<bool> UpdateUserJoinDate(Snowflake snowflake, DateTime joinDate);
    Task<DateTime?> GetUserBirthday(Snowflake snowflake);
    Task<DateTime?> GetUserJoinDate(Snowflake snowflake);
}