using Discord;

namespace PaxAndromeda.Instar.Services;

public interface IDiscordService
{
    IInstarGuild GetGuild();
    Task<IEnumerable<IGuildUser>> GetAllUsers();
    Task<IChannel> GetChannel(Snowflake channelId);
}