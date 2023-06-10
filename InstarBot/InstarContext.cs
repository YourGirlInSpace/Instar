using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using PaxAndromeda.Instar.Wrappers;

namespace PaxAndromeda.Instar;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class InstarContext : IInteractionContext
{
    private readonly SocketInteractionContext _context = null!;

    internal InstarContext(SocketInteractionContext context)
    {
        _context = context;
    }
    
    [UsedImplicitly]
    public InstarContext()
    { }

    public IDiscordClient Client => ((IInteractionContext)_context).Client;

    public virtual IDiscordInteraction Interaction => _context.Interaction;

    protected internal virtual IGuildUser? User => _context.User as IGuildUser;
    IUser IInteractionContext.User => User!;

    protected internal virtual IGuildChannel? Channel => _context.Channel as IGuildChannel;
    IMessageChannel IInteractionContext.Channel => (Channel as IMessageChannel)!;

    protected internal virtual IInstarGuild Guild => new SocketGuildWrapper(_context.Guild);
    IGuild IInteractionContext.Guild => _context.Guild;
}