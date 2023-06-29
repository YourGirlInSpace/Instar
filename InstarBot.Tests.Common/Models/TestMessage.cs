using System.Diagnostics.CodeAnalysis;
using Discord;
using PaxAndromeda.Instar;

namespace InstarBot.Tests.Models;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class TestMessage : IMessage
{

    internal TestMessage(IUser user, string message)
    {
        Id = Snowflake.Generate();
        CreatedAt = DateTimeOffset.Now;
        Timestamp = DateTimeOffset.Now;
        Author = user;

        Content = message;
    }

    public ulong Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Task DeleteAsync(RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public Task AddReactionAsync(IEmote emote, RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAllReactionsAsync(RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit,
        RequestOptions options = null!)
    {
        throw new NotImplementedException();
    }

    public MessageType Type { get; set; } = default;
    public MessageSource Source { get; set; } = default;
    public bool IsTTS { get; set; } = default;
    public bool IsPinned { get; set; } = default;
    public bool IsSuppressed { get; set; } = default;
    public bool MentionedEveryone { get; set; } = default;
    public string Content { get; set; }
    public string CleanContent { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset? EditedTimestamp { get; set; } = default;
    public IMessageChannel Channel { get; set; } = default!;
    public IUser Author { get; set; }
    public IThreadChannel Thread { get; set; } = default!;
    public IReadOnlyCollection<IAttachment> Attachments { get; set; } = default!;
    public IReadOnlyCollection<IEmbed> Embeds { get; set; } = default!;
    public IReadOnlyCollection<ITag> Tags { get; set; } = default!;
    public IReadOnlyCollection<ulong> MentionedChannelIds { get; set; } = default!;
    public IReadOnlyCollection<ulong> MentionedRoleIds { get; set; } = default!;
    public IReadOnlyCollection<ulong> MentionedUserIds { get; set; } = default!;
    public MessageActivity Activity { get; set; } = default!;
    public MessageApplication Application { get; set; } = default!;
    public MessageReference Reference { get; set; } = default!;
    public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; set; } = default!;
    public IReadOnlyCollection<IMessageComponent> Components { get; set; } = default!;
    public IReadOnlyCollection<IStickerItem> Stickers { get; set; } = default!;
    public MessageFlags? Flags { get; set; } = default;
    public IMessageInteraction Interaction { get; set; } = default!;
    public MessageRoleSubscriptionData RoleSubscriptionData { get; set; } = default!;
}