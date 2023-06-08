using Discord;

namespace PaxAndromeda.Instar;

public interface IInstarGuild
{
    ulong Id { get; }

    ITextChannel GetTextChannel(ulong channelId);
}