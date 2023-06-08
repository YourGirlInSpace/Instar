using System.Diagnostics.CodeAnalysis;
using System.Text;
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
        var teamId = target.GetTeamIDs().First();
        _scenarioContext.Add("UserTeamID", teamId);
    }

    [Given("the user is not a staff member")]
    public void GivenTheUserIsNotAStaffMember()
    {
        _scenarioContext.Add("UserTeamID", 0ul);
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
        
        await command.Object.Page(pageTarget, "This is a test reason", teamLead: pagingTeamLeader);
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
            expectedString = "This is a __**TEST**__ page.";
        else
        {
            var teamId = pageTarget.GetTeamIDs().First();
            expectedString = $"<@&{teamId}>";
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
                .ToDictionary(n => n.ID, n => n);

        // Eeeeeeeeeeeeevil
        return teamsConfig![pageTarget.GetTeamIDs().First()].Teamleader;
    }
    
    [Then("Instar should emit a valid All Page embed")]
    public void ThenInstarShouldEmitAValidAllPageEmbed()
    {
        _scenarioContext.ContainsKey("Command").Should().BeTrue();
        var command = _scenarioContext.Get<Mock<PageCommand>>("Command");
        StringBuilder pingBuilder = new();
        foreach (var teamId in PageTarget.All.GetTeamIDs())
            pingBuilder.Append($"<@&{teamId}> ");

        var target = pingBuilder.ToString();
        
        command.Protected().Verify(
            "RespondAsync", Times.Once(),
            target.Length == 0 ? target : target[..^1], ItExpr.IsNull<Embed[]>(),
            false, false, AllowedMentions.All, ItExpr.IsNull<RequestOptions>(),
            ItExpr.IsNull<MessageComponent>(), ItExpr.IsAny<Embed>());
    }
    
    private Mock<PageCommand> SetupMocks()
    {
        var commandMock = TestUtilities.SetupCommandMock<PageCommand>(new CommandMockContext
        {
            UserRoles = new List<ulong> { _scenarioContext.Get<ulong>("UserTeamID") }
        });

        return commandMock;
    }
}