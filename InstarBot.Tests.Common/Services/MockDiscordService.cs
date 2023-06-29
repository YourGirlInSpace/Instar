using Discord;
using InstarBot.Tests.Models;
using JetBrains.Annotations;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Services;

namespace InstarBot.Tests.Services;

public sealed class MockDiscordService : IDiscordService
{
    private readonly IInstarGuild _guild;
    private readonly AsyncEvent<IGuildUser> _userJoinedEvent = new();
    private readonly AsyncEvent<IMessage> _messageReceivedEvent = new();
    private readonly AsyncEvent<Snowflake> _messageDeletedEvent = new();

    public event Func<IGuildUser, Task> UserJoined
    {
        add => _userJoinedEvent.Add(value);
        remove => _userJoinedEvent.Remove(value);
    }

    public event Func<IMessage, Task> MessageReceived
    {
        add => _messageReceivedEvent.Add(value);
        remove => _messageReceivedEvent.Remove(value);
    }

    public event Func<Snowflake, Task> MessageDeleted
    {
        add => _messageDeletedEvent.Add(value);
        remove => _messageDeletedEvent.Remove(value);
    }
    
    internal MockDiscordService(IInstarGuild guild)
    {
        _guild = guild;
    }

    public Task Start(IServiceProvider provider)
    {
        return Task.CompletedTask;
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

    public async IAsyncEnumerable<IMessage> GetMessages(IInstarGuild guild, DateTime afterTime)
    {
        foreach (var channel in guild.TextChannels)
        await foreach (var messageList in channel.GetMessagesAsync())
        foreach (var message in messageList)
            yield return message;
    }

    public async Task TriggerUserJoined(IGuildUser user)
    {
        await _userJoinedEvent.Invoke(user);
    }

    [UsedImplicitly]
    public async Task TriggerMessageReceived(IMessage message)
    {
        await _messageReceivedEvent.Invoke(message);
    }
}