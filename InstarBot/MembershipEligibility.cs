namespace PaxAndromeda.Instar;

[Flags]
public enum MembershipEligibility
{
    Eligible = 0x0,
    NotEligible = 0x1,
    AlreadyMember = 0x2,
    TooYoung = 0x4,
    MissingRoles = 0x8,
    MissingIntroduction = 0x10,
    PunishmentReceived = 0x20,
    NotEnoughMessages = 0x40
}