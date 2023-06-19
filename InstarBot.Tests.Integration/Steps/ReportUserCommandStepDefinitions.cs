using Discord;
using FluentAssertions;
using Moq;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Commands;
using PaxAndromeda.Instar.Modals;
using TechTalk.SpecFlow.Assist;
using Xunit;

namespace InstarBot.Tests.Integration;

[Binding]
public class ReportUserCommandStepDefinitions
{
    private readonly ScenarioContext _context;

    public ReportUserCommandStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [When(@"the user (.*) reports a message with the following properties")]
    public async Task WhenTheUserReportsAMessageWithTheFollowingProperties(ulong userId, Table table)
    {
        _context.Add("ReportingUserID", userId);

        var messageProperties = table.Rows.ToDictionary(n => n["Key"], n => n);

        Assert.True(messageProperties.ContainsKey("Content"));
        Assert.True(messageProperties.ContainsKey("Sender"));
        Assert.True(messageProperties.ContainsKey("Channel"));

        _context.Add("MessageContent", messageProperties["Content"].GetString("Value"));
        _context.Add("MessageSender", (ulong)messageProperties["Sender"].GetInt64("Value"));
        _context.Add("MessageChannel", (ulong)messageProperties["Channel"].GetInt64("Value"));

        var (command, interactionContext) = SetupMocks();
        _context.Add("Command", command);
        _context.Add("InteractionContext", interactionContext);

        await command.Object.HandleCommand(interactionContext.Object);
    }

    [When(@"does not complete the modal within 5 minutes")]
    public async Task WhenDoesNotCompleteTheModalWithinMinutes()
    {
        Assert.True(_context.ContainsKey("Command"));
        var command = _context.Get<Mock<ReportUserCommand>>("Command");

        ReportUserCommand.PurgeCache();
        _context.Add("ReportReason", string.Empty);

        await command.Object.ModalResponse(new ReportMessageModal
        {
            ReportReason = string.Empty
        });
    }

    [When(@"completes the report modal with reason ""(.*)""")]
    public async Task WhenCompletesTheReportModalWithReason(string reportReason)
    {
        Assert.True(_context.ContainsKey("Command"));
        var command = _context.Get<Mock<ReportUserCommand>>("Command");
        _context.Add("ReportReason", reportReason);

        await command.Object.ModalResponse(new ReportMessageModal
        {
            ReportReason = reportReason
        });
    }

    [Then(@"Instar should emit a message report embed")]
    public void ThenInstarShouldEmitAMessageReportEmbed()
    {
        Assert.True(_context.ContainsKey("TextChannelMock"));
        var textChannel = _context.Get<Mock<ITextChannel>>("TextChannelMock");

        textChannel.Verify(n => n.SendMessageAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(),
            It.IsAny<RequestOptions>(),
            It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(), It.IsAny<MessageComponent>(),
            It.IsAny<ISticker[]>(),
            It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()));

        Assert.True(_context.ContainsKey("ResultEmbed"));
        var embed = _context.Get<Embed>("ResultEmbed");

        embed.Author.Should().NotBeNull();
        embed.Footer.Should().NotBeNull();
        embed.Footer!.Value.Text.Should().Be("Instar Message Reporting System");
    }

    private (Mock<ReportUserCommand>, Mock<IInstarMessageCommandInteraction>) SetupMocks()
    {
        var commandMockContext = new TestContext
        {
            UserID = _context.Get<ulong>("ReportingUserID"),
            EmbedCallback = embed => _context.Add("ResultEmbed", embed)
        };

        var commandMock =
            TestUtilities.SetupCommandMock<ReportUserCommand>(commandMockContext);
        _context.Add("TextChannelMock", commandMockContext.TextChannelMock);

        return (commandMock, SetupMessageCommandMock());
    }

    private Mock<IInstarMessageCommandInteraction> SetupMessageCommandMock()
    {
        var userMock = TestUtilities.SetupUserMock<IGuildUser>(_context.Get<ulong>("ReportingUserID"));
        var authorMock = TestUtilities.SetupUserMock<IGuildUser>(_context.Get<ulong>("MessageSender"));

        var channelMock = TestUtilities.SetupChannelMock<ITextChannel>(_context.Get<ulong>("MessageChannel"));

        var messageMock = new Mock<IMessage>();
        messageMock.Setup(n => n.Id).Returns(100);
        messageMock.Setup(n => n.Author).Returns(authorMock.Object);
        messageMock.Setup(n => n.Channel).Returns(channelMock.Object);

        var socketMessageDataMock = new Mock<IMessageCommandInteractionData>();
        socketMessageDataMock.Setup(n => n.Message).Returns(messageMock.Object);

        var socketMessageCommandMock = new Mock<IInstarMessageCommandInteraction>();
        socketMessageCommandMock.Setup(n => n.User).Returns(userMock.Object);
        socketMessageCommandMock.Setup(n => n.Data).Returns(socketMessageDataMock.Object);

        socketMessageCommandMock.Setup<Task>(n =>
                n.RespondWithModalAsync<ReportMessageModal>(It.IsAny<string>(), It.IsAny<RequestOptions>(),
                    It.IsAny<Action<ModalBuilder>>()))
            .Returns(Task.CompletedTask);

        return socketMessageCommandMock;
    }
}