@interactions
Feature: Set Birthday Command
    
    The Set Birthday command allows users to set
    their own birthdays, which is used within the
    Birthday system.

    Scenario: User should be able to set a valid birthday
        Given the user provides the following parameters
          | Key   | Value |
          | Year  | 1992  |
          | Month | 7     |
          | Day   | 21    |
        When the user calls the Set Birthday command
        Then Instar should emit an ephemeral message stating "Your birthday was set to Tuesday, July 21, 1992."
        And DynamoDB should have the user's Birthday set to 1992-07-21T00:00:00-00:00

    # Note: Update this in 13 years

    Scenario: Underage user should be able to set a valid birthday
        Given the user provides the following parameters
          | Key   | Value |
          | Year  | 2022  |
          | Month | 7     |
          | Day   | 21    |
        When the user calls the Set Birthday command
        Then Instar should emit an ephemeral message stating "Your birthday was set to Thursday, July 21, 2022."
        And DynamoDB should have the user's Birthday set to 2022-07-21T00:00:00-00:00

    Scenario: User should be able to set a valid birthday with time zones
        Given the user provides the following parameters
          | Key      | Value |
          | Year     | 1992  |
          | Month    | 7     |
          | Day      | 21    |
          | Timezone | -8    |
        When the user calls the Set Birthday command
        Then Instar should emit an ephemeral message stating "Your birthday was set to Tuesday, July 21, 1992."
        And DynamoDB should have the user's Birthday set to 1992-07-21T00:00:00-08:00

    Scenario: Attempting to set an illegal day number should emit an error message.
        Given the user provides the following parameters
          | Key   | Value |
          | Year  | 1992  |
          | Month | 2     |
          | Day   | 31    |
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