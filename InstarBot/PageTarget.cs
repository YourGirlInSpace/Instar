using System.Runtime.Serialization;
using Discord.Interactions;

namespace PaxAndromeda.Instar;

public enum PageTarget
{
    Test,
#if DEBUG
    [TeamID(1113478543599468584)]
    Owner,
    [TeamID(1113478785292062800)]
    Admin,
    [TeamID(1113478610825773107)]
    Moderator,
    [TeamID(1113478650768150671)]
    Helper,
    [TeamID(1113478706250395759)]
    [ChoiceDisplay("Community Manager")]
    CommunityManager,

    [TeamID(1113478543599468584)]
    [TeamID(1113478785292062800)]
    [TeamID(1113478610825773107)]
    [TeamID(1113478650768150671)]
    [TeamID(1113478706250395759)]
    All
#else
    [TeamID(793607464833908779)]
    Owner,
    [TeamID(985550619844694037)]
    Admin,
    [TeamID(820350564931338260)]
    Moderator,
    [TeamID(801889809211064390)]
    Helper,
    [TeamID(957411837920567356)]
    [ChoiceDisplay("Community Manager")]
    CommunityManager,

    [TeamID(793607464833908779)]
    [TeamID(985550619844694037)]
    [TeamID(820350564931338260)]
    [TeamID(801889809211064390)]
    [TeamID(957411837920567356)]
    All
#endif
}