using Discord;
using InstarBot.Tests.Models;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Services;

namespace InstarBot.Tests.Services;

public class MockDiscordService : IDiscordService
{
    private readonly IInstarGuild _guild;

    internal MockDiscordService(IInstarGuild guild)
    {
        _guild = guild;
    }

    public IInstarGuild GetGuild()
    {
        return _guild;
    }

    public Task<IEnumerable<IGuildUser>> GetAllUsers()
    {
        return Task.FromResult(((TestGuild)_guild).Users);
    }

    public Task<IChannel> GetChannel(Snowflake channelId)
    {
        return Task.FromResult(_guild.GetTextChannel(channelId) as IChannel);
    }
}