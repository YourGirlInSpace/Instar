using Discord;

namespace PaxAndromeda.Instar;

public interface IInstarGuild
{
    ulong Id { get; }
    IEnumerable<ITextChannel> TextChannels { get; }

    ITextChannel GetTextChannel(ulong channelId);
    IRole GetRole(Snowflake newMemberRole);
}