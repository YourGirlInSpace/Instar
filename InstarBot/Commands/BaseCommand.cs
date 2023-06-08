using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PaxAndromeda.Instar.Wrappers;

namespace PaxAndromeda.Instar.Commands;

/// <summary>
/// Provides a set of methods to obtain context information
/// </summary>
// These methods are actually overridden in Moq
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class BaseCommand : InteractionModuleBase<SocketInteractionContext>
{
    protected internal virtual IGuildUser? User => Context.User as IGuildUser;
    protected internal virtual IGuildChannel? Channel => Context.Channel as IGuildChannel;
    protected internal virtual IInstarGuild Guild => new SocketGuildWrapper(Context.Guild);
}