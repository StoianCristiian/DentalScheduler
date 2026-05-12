using System.Security.Claims;
using DentalScheduler.Api.Controllers;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DentalScheduler.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IAwsS3Service> _awsS3ServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        _identityServiceMock = new Mock<IIdentityService>();
        _awsS3ServiceMock = new Mock<IAwsS3Service>();

        _controller = new AuthController(_userManagerMock.Object, _identityServiceMock.Object, _awsS3ServiceMock.Object);
        
        // Mocking user principal
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Register_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@test.com", Password = "Password123!", FirstName = "Test", LastName = "User" };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_WhenFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@test.com", Password = "Password123!", FirstName = "Test", LastName = "User" };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Me_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.Me();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Me_WhenAuthorized_ShouldReturnOkProfile()
    {
        // Arrange
        var appUser = new ApplicationUser { Id = "user-id", Email = "test@test.com", FirstName = "F", LastName = "L", ProfilePictureUrl = "s3key" };
        _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(appUser);
        _userManagerMock.Setup(x => x.GetRolesAsync(appUser))
            .ReturnsAsync(new List<string> { "Patient" });
        _awsS3ServiceMock.Setup(x => x.GetPresignedUrlAsync("s3key", It.IsAny<int>()))
            .ReturnsAsync("http://presigned-url");

        // Act
        var result = await _controller.Me();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }
}
