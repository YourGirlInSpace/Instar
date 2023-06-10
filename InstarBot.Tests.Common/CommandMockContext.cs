using System.Diagnostics.CodeAnalysis;
using Discord;
using Moq;
using PaxAndromeda.Instar;

namespace InstarBot.Tests;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class CommandMockContext
{
    public ulong UserID { get; init; } = 100;
    public ulong ChannelID { get; init; } = 200;
    public ulong GuildID { get; init; } = 300;

    public List<Snowflake> UserRoles { get; init; } = new();

    public Action<Embed> EmbedCallback { get; init; } = _ => { };

    public Mock<ITextChannel> TextChannelMock { get; internal set; } = null!;
}