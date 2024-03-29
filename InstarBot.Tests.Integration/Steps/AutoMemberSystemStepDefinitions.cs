using Discord;
using FluentAssertions;
using InstarBot.Tests.Models;
using InstarBot.Tests.Services;
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

    private async Task<AutoMemberSystem> SetupTest()
    {
        var context = _scenarioContext.Get<TestContext>("Context");
        var discordService = TestUtilities.SetupDiscordService(context);
        var gaiusApiService = TestUtilities.SetupGaiusAPIService(context);
        var config = TestUtilities.GetDynamicConfiguration();
        _scenarioContext.Add("Config", config);
        _scenarioContext.Add("DiscordService", discordService);
        
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var relativeJoinTime = _scenarioContext.Get<int>("UserAge");
        var roles = _scenarioContext.Get<ulong[]>("UserRoles").Select(roleId => new Snowflake(roleId)).ToArray();
        var postedIntro = _scenarioContext.Get<bool>("UserPostedIntroduction");
        var messagesLast24Hours = _scenarioContext.Get<int>("UserMessagesPast24Hours");
        var firstSeenTime = _scenarioContext.ContainsKey("UserFirstJoinedTime") ? _scenarioContext.Get<int>("UserFirstJoinedTime") : 0;
        var grantedMembershipBefore = _scenarioContext.ContainsKey("UserGrantedMembershipBefore") && _scenarioContext.Get<bool>("UserGrantedMembershipBefore");
        var amsConfig = _scenarioContext.Get<AutoMemberConfig>("AMSConfig");

        var ddbService = new MockInstarDDBService();
        if (firstSeenTime > 0)
            await ddbService.UpdateUserJoinDate(userId, DateTime.UtcNow - TimeSpan.FromHours(firstSeenTime));
        if (grantedMembershipBefore)
            await ddbService.UpdateUserMembership(userId, grantedMembershipBefore);
        
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
        

        var ams = new AutoMemberSystem(config, discordService, gaiusApiService, ddbService, new MockMetricService());
        _scenarioContext.Add("AutoMemberSystem", ams);
        _scenarioContext.Add("User", user);

        return ams;
    }

    [When("the Auto Member System processes")]
    public async Task WhenTheAutoMemberSystemProcesses()
    {
        var ams = await SetupTest();

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
            ModID = Snowflake.Generate(),
            UserID = userId
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
            ModID = Snowflake.Generate(),
            UserID = userId
        });
    }

    [Given("the Gaius API is not available")]
    public void GivenTheGaiusApiIsNotAvailable()
    {
        var context = _scenarioContext.Get<TestContext>("Context");
        context.InhibitGaius = true;
    }

    [Given("a user that has:")]
    public async Task GivenAUserThatHas()
    {
        var config = await TestUtilities.GetDynamicConfiguration().GetConfig();
        var amsConfig = config.AutoMemberConfig;
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
    public async Task ThenTheUserShouldBeGrantedMembership()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");
        var config = _scenarioContext.Get<IDynamicConfigService>("Config");
        var user = context.GuildUsers.First(n => n.Id == userId.ID);

        var cfg = await config.GetConfig();

        user.RoleIds.Should().Contain(cfg.MemberRoleID);
        user.RoleIds.Should().NotContain(cfg.NewMemberRoleID);
    }

    [Then("the user should not be granted membership")]
    public async Task ThenTheUserShouldNotBeGrantedMembership()
    {
        var userId = _scenarioContext.Get<Snowflake>("UserID");
        var context = _scenarioContext.Get<TestContext>("Context");
        var config = _scenarioContext.Get<IDynamicConfigService>("Config");
        var user = context.GuildUsers.First(n => n.Id == userId.ID);

        var cfg = await config.GetConfig();
        
        user.RoleIds.Should().Contain(cfg.NewMemberRoleID);
        user.RoleIds.Should().NotContain(cfg.MemberRoleID);
    }

    [Given("Not been punished")]
    public void GivenNotBeenPunished()
    {
        // ignore
    }

    [Given(@"First joined (.*) hours ago")]
    public void GivenFirstJoinedHoursAgo(int hoursAgo) => _scenarioContext.Add("UserFirstJoinedTime", hoursAgo);

    [Given(@"Joined the server for the first time")]
    public void GivenJoinedTheServerForTheFirstTime() => _scenarioContext.Add("UserFirstJoinedTime", 0);

    [Given(@"Been granted membership before")]
    public void GivenBeenGrantedMembershipBefore() => _scenarioContext.Add("UserGrantedMembershipBefore", true);

    [Given(@"Not been granted membership before")]
    public void GivenNotBeenGrantedMembershipBefore() => _scenarioContext.Add("UserGrantedMembershipBefore", false);

    [When(@"the user joins the server")]
    public async Task WhenTheUserJoinsTheServer()
    {
        await SetupTest();
        var service = _scenarioContext.Get<IDiscordService>("DiscordService") as MockDiscordService;
        var user = _scenarioContext.Get<IGuildUser>("User");

        service?.TriggerUserJoined(user);
    }
}