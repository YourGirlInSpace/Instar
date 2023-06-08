using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PaxAndromeda.Instar.Wrappers;

namespace PaxAndromeda.Instar.Commands;

// These methods are actually overridden in Moq
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class BaseCommand : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual IGuildUser? GetUser()
    {
        return Context.User as IGuildUser;
    }
    
    protected virtual IGuildChannel? GetChannel()
    {
        return Context.Channel as IGuildChannel;
    }
    
    protected virtual IInstarGuild GetGuild()
    {
        return new SocketGuildWrapper(Context.Guild);
    }
}