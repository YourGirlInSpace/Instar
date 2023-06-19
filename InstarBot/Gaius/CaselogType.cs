using System.Runtime.Serialization;

namespace PaxAndromeda.Instar.Gaius;

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
    Timeout
}