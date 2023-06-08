Feature: PingCommand
	Simple calculator for adding two numbers

Scenario: User should be able to issue the Ping command.
	When the user calls the Ping command
	Then Instar should emit an ephemeral message stating "Pong!"