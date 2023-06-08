Feature: SetBirthdayCommand
Test set for the Page command.

Scenario: User should be able to set a valid birthday
    Given the user provides the following parameters
      | Key      | Value |
      | Year     | 1992  |
      | Month    | 7     |
      | Day      | 21    |
    When the user calls the Set Birthday command
    Then Instar should emit an ephemeral message stating "Your birthday was set to Tuesday, July 21, 1992."
        
Scenario: User should be able to set a valid birthday with time zones
    Given the user provides the following parameters
      | Key      | Value |
      | Year     | 1992  |
      | Month    | 7     |
      | Day      | 21    |
      | Timezone | -8     |
    When the user calls the Set Birthday command
    Then Instar should emit an ephemeral message stating "Your birthday was set to Tuesday, July 21, 1992."

Scenario: Attempting to set an illegal day number should emit an error message.
    Given the user provides the following parameters
      | Key      | Value |
      | Year     | 1992  |
      | Month    | 2     |
      | Day      | 31    |
    When the user calls the Set Birthday command
    Then Instar should emit an ephemeral message stating "There are only 29 days in February 1992.  Your birthday was not set."
    
Scenario: Attempting to set a birthday in the future should emit an error message
    Given the user provides the following parameters
      | Key   | Value |
      | Year  | 9992  |
      | Month | 2     |
      | Day   | 21    |
    When the user calls the Set Birthday command
    Then Instar should emit an ephemeral message stating "You are not a time traveler.  Your birthday was not set."