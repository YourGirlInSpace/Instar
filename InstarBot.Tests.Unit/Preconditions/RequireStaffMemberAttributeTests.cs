using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using PaxAndromeda.Instar;
using PaxAndromeda.Instar.Preconditions;
using Xunit;

namespace InstarBot.Tests.Preconditions;

public class RequireStaffMemberAttributeTests
{
    [Fact]
    public async Task CheckRequirementsAsync_ShouldReturnSuccessful_WithValidUser()
    {
        // Arrange
        var attr = new RequireStaffMemberAttribute();

        var context = TestUtilities.SetupContext(new CommandMockContext
        {
            UserRoles = new List<Snowflake>
            {
                new(793607635608928257)
            }
        });

        // Act
        var result = await attr.CheckRequirementsAsync(context.Object, null!, TestUtilities.GetServices());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckRequirementsAsync_ShouldReturnFailure_WithNonStaffUser()
    {
        // Arrange
        var attr = new RequireStaffMemberAttribute();

        var context = TestUtilities.SetupContext(new CommandMockContext
        {
            UserRoles = new List<Snowflake>
            {
                new()
            }
        });

        // Act
        var result = await attr.CheckRequirementsAsync(context.Object, null!, TestUtilities.GetServices());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorReason.Should().Be("You are not eligible to run this command");
    }
}