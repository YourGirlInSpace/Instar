Feature: PageCommand
	Test set for the Page command.

Scenario: User should be able to page when authorized
	Given the user is in team Owner
	And the user is paging Moderator
	When the user calls the Page command
	Then Instar should emit a valid Page embed
	
Scenario: User should be able to page a team's teamleader
	Given the user is in team Helper
	And the user is paging the Owner teamleader
	When the user calls the Page command
	Then Instar should emit a valid teamleader Page embed

Scenario: Any staff member should be able to use the Test page
	Given the user is in team Helper
	And the user is paging Test
	When the user calls the Page command
	Then Instar should emit a valid Page embed

Scenario: Owner should be able to page all
	Given the user is in team Owner
	And the user is paging All
	When the user calls the Page command
	Then Instar should emit a valid All Page embed
	
	
Scenario: Fail page if paging all teamleader
	Given the user is in team Owner
	And the user is paging the All teamleader
	When the user calls the Page command
	Then Instar should emit an ephemeral message stating "Failed to send page.  The 'All' team does not have a teamleader.  If intended to page the owner, please select the Owner as the team."
	
Scenario: Unauthorized user should receive an error message
	Given the user is not a staff member
	And the user is paging Moderator
	When the user calls the Page command
	Then Instar should emit an ephemeral message stating "You are not authorized to use this command."

Scenario: Helper should not be able to page all
	Given the user is in team Helper
	And the user is paging All
	When the user calls the Page command
	Then Instar should emit an ephemeral message stating "You are not authorized to send a page to the entire staff team."