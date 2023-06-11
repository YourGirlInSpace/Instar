using PaxAndromeda.Instar.Commands;

namespace InstarBot.Tests.Integration;

[Binding]
public class PingCommandStepDefinitions
{
    private readonly ScenarioContext _context;

    public PingCommandStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [When(@"the user calls the Ping command")]
    public async Task WhenTheUserCallsThePingCommand()
    {
        var command = TestUtilities.SetupCommandMock<PingCommand>();
        _context.Add("Command", command);

        await command.Object.Ping();
    }
}