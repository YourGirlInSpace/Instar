using Xunit;

namespace InstarBot.Tests.Integration.Hooks;

[Binding]
public class InstarHooks
{
    private readonly ScenarioContext _scenarioContext;

    public InstarHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then(@"Instar should emit a message stating ""(.*)""")]
    public void ThenInstarShouldEmitAMessageStating(string message)
    {
        Assert.True(_scenarioContext.ContainsKey("Command"));
        var cmdObject = _scenarioContext.Get<object>("Command");
        TestUtilities.VerifyMessage(cmdObject, message);
    }

    [Then(@"Instar should emit an ephemeral message stating ""(.*)""")]
    public void ThenInstarShouldEmitAnEphemeralMessageStating(string message)
    {
        Assert.True(_scenarioContext.ContainsKey("Command"));
        var cmdObject = _scenarioContext.Get<object>("Command");
        TestUtilities.VerifyMessage(cmdObject, message, true);
    }
}