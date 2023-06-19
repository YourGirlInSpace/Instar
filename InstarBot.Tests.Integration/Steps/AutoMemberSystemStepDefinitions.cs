using FluentAssertions;
using InstarBot.Tests.Models;
using Microsoft.Extensions.Configuration;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.ConfigModels;
using PaxAndromeda.Instar.Gaius;
using PaxAndromeda.Instar.Services;

namespace InstarBot.Tests.Integration;

[Binding]
public class AutoMemberSystemStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Dictionary<string, ulong> _roleNameIDMap = new();

    public AutoMemberSystemStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given("the roles as follows:")]
    public void GivenTheRolesAsFollows(Table table)
    {
        foreach (var row in table.Rows)
        {
            _roleNameIDMap.Add(row["Role Name"], ulong.Parse(row["Role ID"]));
        }
    }

    [When("the Auto Member System processes")]
    public async Task WhenTheAutoMemberSystemProcesses()
    {
        var context = _scenarioContext.Get<TestContext>("Context");
        var discordService = TestUtilities.SetupDiscordService(context);
        var gaiusApiService = TestUtilities.SetupGaiusAPIService(context);
        var config = TestUtilities.GetTestConfiguration();
        _scenarioContext.Add("Config", config);
        
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var relativeJoinTime = _scenarioContext.Get<int>("UserAge");
        var roles = _scenarioContext.Get<ulong[]>("UserRoles").Select(roleId => new Snowflake(roleId)).ToArray();
        var postedIntro = _scenarioContext.Get<bool>("UserPostedIntroduction");
        var messagesLast24Hours = _scenarioContext.Get<int>("UserMessagesPast24Hours");
        var amsConfig = _scenarioContext.Get<AutoMemberConfig>("AMSConfig");

        context.AddRoles(roles);

        var user = new TestGuildUser
        {
            Id = userId,
            JoinedAt = DateTimeOffset.Now - TimeSpan.FromHours(relativeJoinTime),
            RoleIds = roles.Select(n => n.ID).ToList().AsReadOnly()
        };

        context.GuildUsers.Add(user);


        var genericChannel = Snowflake.Generate();
        context.AddChannel(amsConfig.IntroductionChannel);
        
        context.AddChannel(genericChannel);
        if (postedIntro)
            context.GetChannel(amsConfig.IntroductionChannel).AddMessage(user, "Some text");
            
        for (var i = 0; i < messagesLast24Hours; i++)
            context.GetChannel(genericChannel).AddMessage(user, "Some text");
        

        var ams = new AutoMemberSystem(config, discordService, gaiusApiService);
        _scenarioContext.Add("AutoMemberSystem", ams);

        await ams.RunAsync();
    }

    [Then("the user should remain unchanged")]
    public void ThenTheUserShouldRemainUnchanged()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");
        var user = context.GuildUsers.First(n => n.Id == userId.ID) as TestGuildUser;

        user.Should().NotBeNull();
        user!.Changed.Should().BeFalse();
    }

    [Given("Been issued a warning")]
    public void GivenBeenIssuedAWarning()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");

        context.AddWarning(userId, new Warning
        {
            Reason = "TEST WARNING",
            ModID = Snowflake.Generate()
        });
    }

    [Given("Been issued a mute")]
    public void GivenBeenIssuedAMute()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");

        context.AddCaselog(userId, new Caselog
        {
            Type = CaselogType.Mute,
            Reason = "TEST WARNING",
            ModID = Snowflake.Generate()
        });
    }

    [Given("the Gaius API is not available")]
    public void GivenTheGaiusApiIsNotAvailable()
    {
        var context = _scenarioContext.Get<TestContext>("Context");
        context.InhibitGaius = true;
    }

    [Given("a user that has:")]
    public void GivenAUserThatHas()
    {
        var config = TestUtilities.GetTestConfiguration();
        var amsConfig = config.GetSection("AutoMemberConfig").Get<AutoMemberConfig>()!;
        _scenarioContext.Add("AMSConfig", amsConfig);
        
        var cmc = new TestContext();
        _scenarioContext.Add("Context", cmc);
        
        var userId = Snowflake.Generate();
        _scenarioContext.Add("UserID", userId);
    }

    [Given("Joined (.*) hours ago")]
    public void GivenJoinedHoursAgo(int ageHours) => _scenarioContext.Add("UserAge", ageHours);

    [Given("The roles (.*)")]
    public void GivenTheRoles(string roles)
    {
        var roleNames = roles.Split(new[] { ",", "and" },
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var roleIds = roleNames.Select(roleName => _roleNameIDMap[roleName]).ToArray();
        
        _scenarioContext.Add("UserRoles", roleIds);
    }

    [Given("Posted an introduction")]
    public void GivenPostedAnIntroduction() => _scenarioContext.Add("UserPostedIntroduction", true);

    [Given("Did not post an introduction")]
    public void GivenDidNotPostAnIntroduction() => _scenarioContext.Add("UserPostedIntroduction", false);

    [Given("Posted (.*) messages in the past day")]
    public void GivenPostedMessagesInThePastDay(int numMessages) => _scenarioContext.Add("UserMessagesPast24Hours", numMessages);

    [Then("the user should be granted membership")]
    public void ThenTheUserShouldBeGrantedMembership()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");
        var config = _scenarioContext.Get<IConfiguration>("Config");
        var user = context.GuildUsers.First(n => n.Id == userId.ID);

        user.RoleIds.Should().Contain(config.GetValue<ulong>("MemberRoleID"));
        user.RoleIds.Should().NotContain(config.GetValue<ulong>("NewMemberRoleID"));
    }

    [Then("the user should not be granted membership")]
    public void ThenTheUserShouldNotBeGrantedMembership()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");
        var config = _scenarioContext.Get<IConfiguration>("Config");
        var user = context.GuildUsers.First(n => n.Id == userId.ID);

        user.RoleIds.Should().Contain(config.GetValue<ulong>("NewMemberRoleID"));
        user.RoleIds.Should().NotContain(config.GetValue<ulong>("MemberRoleID"));
    }

    [Given(@"Not been punished")]
    public void GivenNotBeenPunished()
    {
        // ignore
    }
}