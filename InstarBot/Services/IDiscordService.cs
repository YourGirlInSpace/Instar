using Discord;

namespace PaxAndromeda.Instar.Services;

public interface IDiscordService
{
    event Func<IGuildUser, Task> UserJoined;
    event Func<IMessage, Task> MessageReceived;
    event Func<Snowflake, Task> MessageDeleted;
    
    Task Start(IServiceProvider provider);
    IInstarGuild GetGuild();
    Task<IEnumerable<IGuildUser>> GetAllUsers();
    Task<IChannel> GetChannel(Snowflake channelId);
    IAsyncEnumerable<IMessage> GetMessages(IInstarGuild guild, DateTime afterTime);
}