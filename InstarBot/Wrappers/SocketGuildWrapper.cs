using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace PaxAndromeda.Instar.Wrappers;

/// <summary>
/// Mock wrapper for <see cref="SocketGuild"/>
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Wrapper class")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class SocketGuildWrapper : IInstarGuild
{
    private readonly SocketGuild _guild;

    public SocketGuildWrapper(SocketGuild guild)
    {
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
    }

    public virtual ulong Id => _guild.Id;
    public IEnumerable<ITextChannel> TextChannels => _guild.TextChannels;

    public virtual ITextChannel GetTextChannel(ulong channelId)
    {
        return _guild.GetTextChannel(channelId);
    }

    public IRole GetRole(Snowflake roleId)
    {
        return _guild.GetRole(roleId);
    }
}