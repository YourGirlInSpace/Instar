[![Instar Builds](https://github.com/YourGirlInSpace/Instar/actions/workflows/dotnet-linux.yml/badge.svg)](https://github.com/YourGirlInSpace/Instar/actions/workflows/dotnet-linux.yml) [![Discord Link](https://img.shields.io/discord/789965848215420949?label=Trans+Haven&color=0099FF&logo=Discord)](https://discord.gg/transhaven)

# [<img src="https://spacegirl.s3.us-east-1.amazonaws.com/instar.png" width="48" height="48" style="position: relative; top: 12px;"/>](https://github.com/YourGirlInSpace/Instar/) Instar

Instar is an in-house bot for management and automation of manual workflows historically done in [Trans Haven](https://discord.gg/transhaven).  This bot is not available for public release.

### Packages
- [InstarBot](https://github.com/YourGirlInSpace/Instar/tree/master/InstarBot) contains the core logic of Instar.
- [InstarBot.Tests.Common](https://github.com/YourGirlInSpace/Instar/tree/master/InstarBot.Tests.Common) provides test utilities, mocks and other tools for testing.
- [InstarBot.Tests.Unit](https://github.com/YourGirlInSpace/Instar/tree/master/InstarBot.Tests.Unit) contains unit tests for Instar.
- [InstarBot.Tests.Integration](https://github.com/YourGirlInSpace/Instar/tree/master/InstarBot.Tests.Integration) contains integration and acceptance tests for Instar.

### Configuration
Instar requires configuration to be placed within the `/Config` directory under the directory where `InstarBot` is located.  An example of the config may be found [here](https://github.com/YourGirlInSpace/Instar/blob/master/InstarBot.Tests.Common/Config/Instar.test.conf.json), and its schema is found [here](https://github.com/YourGirlInSpace/Instar/blob/master/InstarBot/Config/Instar.conf.schema.json).

For the purposes of testing, one may omit the AWS, Discord and Gaius API access keys.

### Development
Users wishing to contribute to Instar must file a [Pull Request](https://github.com/YourGirlInSpace/Instar/pulls) for their code to be integrated.  All changes require full unit and/or integration testing and should follow the code style shown within the project.
