using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

public interface IInstarMessageCommandInteraction
{
    /// <summary>Gets the id of the interaction.</summary>
    [UsedImplicitly]
    ulong Id { get; }

    /// <summary>Gets the user who invoked the interaction.</summary>
    IUser User { get; }

    /// <summary>Gets the data associated with this interaction.</summary>
    IMessageCommandInteractionData Data { get; }

    Task RespondWithModalAsync<T>(string customId,
        RequestOptions options = null!,
        Action<ModalBuilder> modifyModal = null!)
        where T : class, IModal;
}