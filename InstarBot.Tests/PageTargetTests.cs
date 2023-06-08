using System;
using FluentAssertions;
using PaxAndromeda.Instar;
using Xunit;

namespace InstarBot.Tests;

public class PageTargetTests
{
    [Fact]
    public void GetTeamIDs_ShouldThrowException_OnTestTarget()
    {
        // Arrange
        const PageTarget testTarget = PageTarget.Test;

        // Act
        Action testAction = () => testTarget.GetTeamIDs();

        // Assert
        testAction.Should().Throw<InvalidStateException>();
    }
    
    [Fact]
    public void GetTeamIDs_ShouldReturnOne_OnOwnerTarget()
    {
        // Arrange
        const PageTarget testTarget = PageTarget.Owner;

        // Act
        var teamIds = testTarget.GetTeamIDs();

        // Assert
        teamIds.Should().HaveCount(1);
    }
    
    [Fact]
    public void GetTeamIDs_ShouldReturnMany_OnAllTarget()
    {
        // Arrange
        const PageTarget testTarget = PageTarget.Owner;

        // Act
        var teamIds = testTarget.GetTeamIDs();

        // Assert
        teamIds.Should().HaveCountGreaterOrEqualTo(1);
    }
}