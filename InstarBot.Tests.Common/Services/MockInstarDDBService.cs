using InstarBot.Tests.Models;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Services;

namespace InstarBot.Tests.Services;

public class MockInstarDDBService : IInstarDDBService
{
    private readonly Dictionary<Snowflake, UserDatabaseInformation> _localData;

    public MockInstarDDBService()
    {
        _localData = new Dictionary<Snowflake, UserDatabaseInformation>();
    }

    public MockInstarDDBService(IEnumerable<UserDatabaseInformation> preload)
    {
        _localData = preload.ToDictionary(n => n.Snowflake, n => n);
    }

    public Task<bool> UpdateUserBirthday(Snowflake snowflake, DateTime birthday)
    {
        _localData.TryAdd(snowflake, new UserDatabaseInformation(snowflake));
        _localData[snowflake].Birthday = birthday;

        return Task.FromResult(true);
    }

    public Task<bool> UpdateUserJoinDate(Snowflake snowflake, DateTime joinDate)
    {
        _localData.TryAdd(snowflake, new UserDatabaseInformation(snowflake));
        _localData[snowflake].Birthday = joinDate;

        return Task.FromResult(true);
    }

    public Task<DateTime?> GetUserBirthday(Snowflake snowflake)
    {
        return !_localData.ContainsKey(snowflake)
            ? Task.FromResult<DateTime?>(null)
            : Task.FromResult<DateTime?>(_localData[snowflake].Birthday);
    }

    public Task<DateTime?> GetUserJoinDate(Snowflake snowflake)
    {
        return !_localData.ContainsKey(snowflake)
            ? Task.FromResult<DateTime?>(null)
            : Task.FromResult<DateTime?>(_localData[snowflake].JoinDate);
    }
}