Feature: ReportUserCommand
Simple calculator for adding two numbers

    Scenario: User should be able to report a message normally
        When the user 1024 reports a message with the following properties
          | Key     | Value                    |
          | Content | "This is a test message" |
          | Sender  | 128                      |
          | Channel | 256                      |
        And completes the report modal with reason "This is a test report"
        Then Instar should emit an ephemeral message stating "Your report has been sent."
        And Instar should emit a message report embed

    Scenario: Report user function times out if cache expires
        When the user 1024 reports a message with the following properties
          | Key     | Value                    |
          | Content | "This is a test message" |
          | Sender  | 128                      |
          | Channel | 256                      |
        And does not complete the modal within 5 minutes
        Then Instar should emit an ephemeral message stating "Report expired.  Please try again."