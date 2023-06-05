using Discord.Interactions;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

public enum PageTarget
{
    Test,
#if DEBUG
    [TeamID(1113478543599468584), UsedImplicitly]
    Owner,
    [TeamID(1113478785292062800), UsedImplicitly]
    Admin,
    [TeamID(1113478610825773107), UsedImplicitly]
    Moderator,
    [TeamID(1113478650768150671)]
    [UsedImplicitly]
    Helper,
    [TeamID(1113478706250395759)]
    [ChoiceDisplay("Community Manager")]
    [UsedImplicitly]
    CommunityManager,

    [TeamID(1113478543599468584)]
    [TeamID(1113478785292062800)]
    [TeamID(1113478610825773107)]
    [TeamID(1113478650768150671)]
    [TeamID(1113478706250395759)]
    [UsedImplicitly]
    All
#else
    [TeamID(793607464833908779), UsedImplicitly]
    Owner,

    [TeamID(985550619844694037), UsedImplicitly]
    Admin,

    [TeamID(820350564931338260), UsedImplicitly]
    Moderator,

    [TeamID(801889809211064390), UsedImplicitly]
    Helper,

    [TeamID(957411837920567356), ChoiceDisplay("Community Manager"), UsedImplicitly]
    CommunityManager,

    [TeamID(793607464833908779), TeamID(985550619844694037), TeamID(820350564931338260), TeamID(801889809211064390), TeamID(957411837920567356)]
    [UsedImplicitly]
    All
#endif
}