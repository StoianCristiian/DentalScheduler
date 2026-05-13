using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Api.Controllers;
using DentalScheduler.Application.Admin.Commands.UpdateUserRole;
using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Admin.Queries.GetDashboardStats;
using DentalScheduler.Application.Admin.Queries.GetUsersWithRoles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
    public async Task GetStats_ReturnsOkWithStats()
    {
        // Arrange
        var stats = new AdminDashboardStatsDto { TotalPatients = 10, TotalDoctors = 5, TotalAppointments = 20 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var model = okResult.Value.Should().BeOfType<AdminDashboardStatsDto>().Subject;
        model.TotalPatients.Should().Be(10);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<UserWithRoleDto> { new UserWithRoleDto { Id = "1", Email = "test@test.com", Role = "Admin" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task UpdateUserRole_Success_ReturnsOk()
    {
        // Arrange
        var request = new UpdateRoleRequest { NewRole = "Admin" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserRole("1", request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUserRole_Failure_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateRoleRequest { NewRole = "Invalid" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateUserRole("1", request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
