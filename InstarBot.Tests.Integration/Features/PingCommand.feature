@interactions
Feature: Ping Command

    The Ping command is a simple mechanism to test
    the responsiveness of the Instar bot.

    Scenario: User should be able to issue the Ping command.
        When the user calls the Ping command
        Then Instar should emit an ephemeral message stating "Pong!"