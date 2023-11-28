using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PaxAndromeda.Instar.Gaius;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum CaselogType
{
    Ban,
    Kick,
    Mute,
    [EnumMember(Value = "vcmute")]
    VoiceMute,
    Softban,
    [EnumMember(Value = "vcban")]
    Voiceban,
    Timeout,
    Unban
}