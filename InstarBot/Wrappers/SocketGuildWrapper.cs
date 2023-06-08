using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace PaxAndromeda.Instar.Wrappers;

/// <summary>
/// Mock wrapper for <see cref="SocketGuild"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class SocketGuildWrapper : IInstarGuild
{
    private readonly SocketGuild _guild;

    public SocketGuildWrapper(SocketGuild guild)
    {
        _guild = guild;
    }

    public virtual ulong Id => _guild.Id;

    public virtual ITextChannel GetTextChannel(ulong channelId)
        => _guild.GetTextChannel(channelId);
}