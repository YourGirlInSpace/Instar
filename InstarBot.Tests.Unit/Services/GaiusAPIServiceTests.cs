using FluentAssertions;
using PaxAndromeda.Instar.Services;
using Xunit;

namespace InstarBot.Tests.Services;

public sealed class GaiusAPIServiceTests
{
    [Fact]
    public void ParseCaselogs_ShouldReturnValidResult_WithNoTotalCasesEntries()
    {
        string caselogsEntry = @"
{
    ""1"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 1
    },
    ""2"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 2
    },
}";

        var result = GaiusAPIService.ParseCaselogs(caselogsEntry);

        result.Should().HaveCount(2);
    }
    
    [Fact]
    public void ParseCaselogs_ShouldReturnValidResult_WithOneTotalCasesEntries()
    {
        string caselogsEntry = @"
{
    ""1"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 1
    },
    ""2"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 2
    },
    ""totalcases"": 3
}";

        var result = GaiusAPIService.ParseCaselogs(caselogsEntry);

        result.Should().HaveCount(2);
    }
    
    [Fact]
    public void ParseCaselogs_ShouldReturnValidResult_WithTwoTotalCasesEntries()
    {
        string caselogsEntry = @"
{
    ""1"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 1
    },
    ""2"": {
        ""userid"": ""1136624753436798986"",
        ""type"": ""ban"",
        ""time"": null,
        ""modid"": ""356878329602768897"",
        ""reason"": ""Posted bad word"",
        ""date"": 1701195012260,
        ""casenum"": 2
    },
    ""totalcases"": 3,
    ""totalCases"": ""2""
}";

        var result = GaiusAPIService.ParseCaselogs(caselogsEntry);

        result.Should().HaveCount(2);
    }
}