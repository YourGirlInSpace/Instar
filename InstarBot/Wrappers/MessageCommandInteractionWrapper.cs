using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;

namespace PaxAndromeda.Instar.Wrappers;

/// <summary>
/// Mock wrapper for <see cref="IMessageCommandInteraction"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class MessageCommandInteractionWrapper : IInstarMessageCommandInteraction
{
    private readonly IMessageCommandInteraction _interaction;

    public MessageCommandInteractionWrapper(IMessageCommandInteraction interaction)
    {
        _interaction = interaction;
    }

    public virtual ulong Id => _interaction.Id;
    public virtual IUser User => _interaction.User;
    public virtual IMessageCommandInteractionData Data => _interaction.Data;

    public virtual Task RespondWithModalAsync<T>(string customId, RequestOptions options = null!,
        Action<ModalBuilder> modifyModal = null!) where T : class, IModal
        => _interaction.RespondWithModalAsync<T>(customId, options, modifyModal);
}