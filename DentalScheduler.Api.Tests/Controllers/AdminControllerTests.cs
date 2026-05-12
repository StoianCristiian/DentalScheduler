using DentalScheduler.Api.Controllers;
using DentalScheduler.Application.Admin.Commands.UpdateUserRole;
using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Admin.Queries.GetDashboardStats;
using DentalScheduler.Application.Admin.Queries.GetUsersWithRoles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DentalScheduler.Api.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AdminController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetStats_ShouldReturnOk_WithStats()
    {
        // Arrange
        var expectedStats = new AdminDashboardStatsDto { TotalAppointments = 5 }; // Ex of mock data
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), default))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<AdminDashboardStatsDto>().Subject;
        stats.Should().BeEquivalentTo(expectedStats);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WithUsersList()
    {
        // Arrange
        var expectedUsers = new List<UserWithRoleDto> { new UserWithRoleDto() };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithRolesQuery>(), default))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var users = okResult.Value.Should().BeAssignableTo<List<UserWithRoleDto>>().Subject;
        users.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task UpdateUserRole_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var userId = "test-id";
        var request = new UpdateRoleRequest { NewRole = "Admin" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserRole(userId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUserRole_WhenFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = "test-id";
        var request = new UpdateRoleRequest { NewRole = "Admin" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateUserRole(userId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}

