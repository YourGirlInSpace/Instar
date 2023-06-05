using System.Runtime.Caching;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PaxAndromeda.Instar.Modals;
using System.Threading.Channels;
using Serilog;

namespace PaxAndromeda.Instar.Commands;

public class ReportUserCommand : InteractionModuleBase<SocketInteractionContext>, IContextCommand
{
    private const string ModalID = "respond_modal";

    public string Name => "Report Message";
    private readonly ulong _staffAnnounceChannel;

    private static readonly MemoryCache _cache = new("User Report Cache"); 

    public ReportUserCommand(IConfiguration config)
    {
        _staffAnnounceChannel = config.GetValue<ulong>("StaffAnnounceChannel");
    }

    public async Task HandleCommand(SocketMessageCommand arg)
    {
        // Cache the message the user is trying to report
        _cache.Set(arg.User.Id.ToString(), arg.Data.Message,
            new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

        await arg.RespondWithModalAsync<ReportMessageModal>(ModalID);
    }

    [ModalInteraction(ModalID)]
    public async Task ModalResponse(ReportMessageModal modal)
    {
        SocketMessage? message = (SocketMessage) _cache.Get(Context.User.Id.ToString());
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (message == null)
        {
            await RespondAsync("Report expired.  Please try again.", ephemeral: true);
            return;
        }

        await SendReportMessage(modal, message, Context.Guild);
        
        await RespondAsync("Your report has been sent.", ephemeral: true);
    }

    private async Task SendReportMessage(ReportMessageModal modal, SocketMessage message, SocketGuild guild)
    {

        var fields = new List<EmbedFieldBuilder>
        {
            new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Message Content")
                .WithValue($"```{message.Content}```"),
            new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Reason")
                .WithValue($"```{modal.ReportReason}```")
        };

        if (message.Author is not null)
            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("User")
                .WithValue($"<@{message.Author.Id}>"));

        if (message.Channel is not null)
            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Channel")
                .WithValue($"<#{message.Channel.Id}>"));

        fields.Add(new EmbedFieldBuilder()
            .WithIsInline(true)
            .WithName("Message")
            .WithValue($"https://discord.com/channels/{guild.Id}/{message.Channel?.Id}/{message.Id}"));

        fields.Add(new EmbedFieldBuilder().WithIsInline(false).WithName("Reported By")
            .WithValue($"<@{Context.User.Id}>"));

        EmbedBuilder builder = new EmbedBuilder()
            // Set up all the basic stuff first
            .WithCurrentTimestamp()
            .WithColor(0x0c94e0)
            .WithAuthor(message.Author?.Username, message.Author?.GetAvatarUrl())
            .WithFooter(new EmbedFooterBuilder()
                .WithText("Instar Message Reporting System")
                .WithIconUrl("https://spacegirl.s3.us-east-1.amazonaws.com/instar.png"))
            .WithFields(fields);

#if DEBUG
        const string staffPing = "{{staffping}}";
#else
        const string staffPing = "<@&793607635608928257>";
#endif

        await
            Context.Guild.GetTextChannel(_staffAnnounceChannel)
                .SendMessageAsync(staffPing, embed: builder.Build());
    }

    public MessageCommandProperties CreateCommand()
    {
        Log.Verbose("Registering ReportUserCommand...");
        var reportMessageCommand = new MessageCommandBuilder()
            .WithName(Name);

        return reportMessageCommand.Build();
    }
}