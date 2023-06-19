using Discord;
using Discord.WebSocket;

namespace PaxAndromeda.Instar.Services;

public interface IDiscordService
{
    Task Start(IServiceProvider provider);
    Task Stop();
    IInstarGuild GetGuild();
    Task<IEnumerable<IGuildUser>> GetAllUsers();
    Task<IChannel> GetChannel(Snowflake channelId);
}