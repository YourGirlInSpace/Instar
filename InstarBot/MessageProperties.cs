using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PaxAndromeda.Instar;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly struct MessageProperties
{
    public readonly ulong UserID;
    public readonly ulong ChannelID;
    public readonly ulong GuildID;

    public MessageProperties(ulong userId, ulong channelId, ulong guildId)
    {
        UserID = userId;
        ChannelID = channelId;
        GuildID = guildId;
    }
}