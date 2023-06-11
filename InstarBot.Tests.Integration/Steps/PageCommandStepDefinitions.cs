using System.Diagnostics.CodeAnalysis;
using Discord;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Commands;

namespace InstarBot.Tests.Integration;

[Binding]
public sealed class PageCommandStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;

    public PageCommandStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given("the user is in team (.*)")]
    public void GivenTheUserIsInTeam(PageTarget target)
    {
        var team = TestUtilities.GetTeams(target).First();
        _scenarioContext.Add("UserTeamID", team.ID);
    }

    [Given("the user is not a staff member")]
    public void GivenTheUserIsNotAStaffMember()
    {
        _scenarioContext.Add("UserTeamID", new Snowflake());
    }

    [Given("the user is paging (CommunityManager|Helper|Moderator|Admin|Owner|Test|All)")]
    [SuppressMessage("ReSharper", "SpecFlow.MethodNameMismatchPattern")]
    public void GivenTheUserIsPaging(PageTarget target)
    {
        _scenarioContext.Add("PageTarget", target);
        _scenarioContext.Add("PagingTeamLeader", false);
    }

    [Given("the user is paging the (CommunityManager|Helper|Moderator|Admin|Owner|Test|All) teamleader")]
    [SuppressMessage("ReSharper", "SpecFlow.MethodNameMismatchPattern")]
    public void GivenTheUserIsPagingTheTeamTeamleader(PageTarget target)
    {
        _scenarioContext.Add("PageTarget", target);
        _scenarioContext.Add("PagingTeamLeader", true);
    }

    [When("the user calls the Page command")]
    public async Task WhenTheUserCallsThePageCommand()
    {
        _scenarioContext.ContainsKey("PageTarget").Should().BeTrue();
        _scenarioContext.ContainsKey("PagingTeamLeader").Should().BeTrue();
        var pageTarget = _scenarioContext.Get<PageTarget>("PageTarget");
        var pagingTeamLeader = _scenarioContext.Get<bool>("PagingTeamLeader");

        var command = SetupMocks();
        _scenarioContext.Add("Command", command);

        await command.Object.Page(pageTarget, "This is a test reason", pagingTeamLeader);
    }

    [Then("Instar should emit a valid Page embed")]
    public void ThenInstarShouldEmitAValidPageEmbed()
    {
        _scenarioContext.ContainsKey("Command").Should().BeTrue();
        _scenarioContext.ContainsKey("PageTarget").Should().BeTrue();
        var command = _scenarioContext.Get<Mock<PageCommand>>("Command");
        var pageTarget = _scenarioContext.Get<PageTarget>("PageTarget");

        string expectedString;

        if (pageTarget == PageTarget.Test)
        {
            expectedString = "This is a __**TEST**__ page.";
        }
        else
        {
            var team = TestUtilities.GetTeams(pageTarget).First();
            expectedString = Snowflake.GetMention(() => team.ID);
        }

        command.Protected().Verify(
            "RespondAsync", Times.Once(),
            expectedString, ItExpr.IsNull<Embed[]>(),
            false, false, AllowedMentions.All, ItExpr.IsNull<RequestOptions>(),
            ItExpr.IsNull<MessageComponent>(), ItExpr.IsAny<Embed>());
    }

    [Then("Instar should emit a valid teamleader Page embed")]
    public void ThenInstarShouldEmitAValidTeamleaderPageEmbed()
    {
        _scenarioContext.ContainsKey("Command").Should().BeTrue();
        _scenarioContext.ContainsKey("PageTarget").Should().BeTrue();

        var command = _scenarioContext.Get<Mock<PageCommand>>("Command");
        var pageTarget = _scenarioContext.Get<PageTarget>("PageTarget");

        command.Protected().Verify(
            "RespondAsync", Times.Once(),
            $"<@{GetTeamLead(pageTarget)}>", ItExpr.IsNull<Embed[]>(),
            false, false, AllowedMentions.All, ItExpr.IsNull<RequestOptions>(),
            ItExpr.IsNull<MessageComponent>(), ItExpr.IsAny<Embed>());
    }

    private static ulong GetTeamLead(PageTarget pageTarget)
    {
        var teamsConfig =
            TestUtilities.GetTestConfiguration().GetSection("Teams").Get<List<Team>>()?
                .ToDictionary(n => n.InternalID, n => n);

        // Eeeeeeeeeeeeevil
        return teamsConfig![pageTarget.GetAttributesOfType<TeamRefAttribute>()?.First().InternalID ?? "idkjustfail"]
            .Teamleader;
    }

    [Then("Instar should emit a valid All Page embed")]
    public void ThenInstarShouldEmitAValidAllPageEmbed()
    {
        _scenarioContext.ContainsKey("Command").Should().BeTrue();
        var command = _scenarioContext.Get<Mock<PageCommand>>("Command");
        var expected = string.Join(' ',
            TestUtilities.GetTeams(PageTarget.All).Select(n => Snowflake.GetMention(() => n.ID)));

        command.Protected().Verify(
            "RespondAsync", Times.Once(),
            expected, ItExpr.IsNull<Embed[]>(),
            false, false, AllowedMentions.All, ItExpr.IsNull<RequestOptions>(),
            ItExpr.IsNull<MessageComponent>(), ItExpr.IsAny<Embed>());
    }

    private Mock<PageCommand> SetupMocks()
    {
        var userTeam = _scenarioContext.Get<Snowflake>("UserTeamID");

        var commandMock = TestUtilities.SetupCommandMock(
            () => new PageCommand(TestUtilities.GetTeamService()),
            new CommandMockContext
            {
                UserRoles = new List<Snowflake> { userTeam }
            });

        return commandMock;
    }
}