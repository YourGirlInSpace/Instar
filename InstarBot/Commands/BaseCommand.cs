using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;

namespace PaxAndromeda.Instar.Commands;

/// <summary>
/// Provides an overridable, testable Context for socket interaction modules
/// </summary>
// These methods are actually overridden in Moq
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class BaseCommand : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>
    /// Overrides the default Context property for testing purposes
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Mockable pointer")]
    protected internal new virtual InstarContext Context => new(base.Context);
}