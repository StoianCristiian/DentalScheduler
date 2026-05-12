using DentalScheduler.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace DentalScheduler.Infrastructure.Tests.Identity;

public class ApplicationUserTests
{
    [Fact]
    public void Properties_Should_Get_And_Set_Correctly()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.FirstName = "Test";
        user.LastName = "User";
        user.ProfilePictureUrl = "http://test.com/pic.jpg";
        user.Address = "123 Test St";

        // Assert
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.ProfilePictureUrl.Should().Be("http://test.com/pic.jpg");
        user.Address.Should().Be("123 Test St");
    }
}

