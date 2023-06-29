using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using InstarBot.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Commands;
using PaxAndromeda.Instar.Services;
using TechTalk.SpecFlow.Assist;
using Xunit;

namespace InstarBot.Tests.Integration;

[Binding]
public class BirthdayCommandStepDefinitions
{
    private readonly ScenarioContext _context;

    public BirthdayCommandStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [Given("the user provides the following parameters")]
    public void GivenTheUserProvidesTheFollowingParameters(Table table)
    {
        var dict = table.Rows.ToDictionary(n => n["Key"], n => n.GetInt32("Value"));

        // Let's see if we have the bare minimum
        Assert.True(dict.ContainsKey("Year") && dict.ContainsKey("Month") && dict.ContainsKey("Day"));

        _context.Add("Year", dict["Year"]);
        _context.Add("Month", dict["Month"]);
        _context.Add("Day", dict["Day"]);

        if (dict.TryGetValue("Timezone", out var value))
            _context.Add("Timezone", value);
    }

    [When("the user calls the Set Birthday command")]
    public async Task WhenTheUserCallsTheSetBirthdayCommand()
    {
        var year = _context.Get<int>("Year");
        var month = _context.Get<int>("Month");
        var day = _context.Get<int>("Day");
        var timezone = _context.ContainsKey("Timezone") ? _context.Get<int>("Timezone") : 0;

        var userId = new Snowflake().ID;

        var ddbService = TestUtilities.GetServices().GetService<IInstarDDBService>();
        var cmd = TestUtilities.SetupCommandMock(() => new SetBirthdayCommand(ddbService!, new MockMetricService()), new TestContext
        {
            UserID = userId
        });
        _context.Add("Command", cmd);
        _context.Add("UserID", userId);
        _context.Add("DDB", ddbService);

        await cmd.Object.SetBirthday((Month)month, day, year, timezone);
    }

    [Then("DynamoDB should have the user's (Birthday|JoinDate) set to (.*)")]
    [SuppressMessage("ReSharper", "SpecFlow.MethodNameMismatchPattern")]
    public async Task ThenDynamoDbShouldHaveBirthdaySetTo(string dataType, DateTime time)
    {
        var ddbService = _context.Get<IInstarDDBService>("DDB");
        var userId = _context.Get<ulong>("UserID");

        switch (dataType)
        {
            case "Birthday":
                (await ddbService!.GetUserBirthday(userId)).Should().Be(time.ToUniversalTime());
                break;
            case "JoinDate":
                (await ddbService!.GetUserJoinDate(userId)).Should().Be(time.ToUniversalTime());
                break;
            default:
                Assert.False(true, "Invalid test setup: dataType is unknown");
                break;
        }
    }
}