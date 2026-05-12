using DentalScheduler.Api.Controllers;
using DentalScheduler.Application.Appointments.Commands.CreateAppointment;
using DentalScheduler.Application.Appointments.Commands.UpdateAppointmentStatus;
using DentalScheduler.Application.Appointments.Queries.GetAppointments;
using DentalScheduler.Application.Appointments.Queries.GetScheduleRecommendations;
using DentalScheduler.Application.DTOs.AI;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DentalScheduler.Api.Tests.Controllers;

public class AppointmentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AppointmentsController _controller;

    public AppointmentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AppointmentsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAppointmentsList()
    {
        // Arrange
        var dummyDto = new AppointmentDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, null, null, null, DentalScheduler.Domain.Enums.AppointmentStatus.Pending, null, null, null, false);
        var expectedAppointments = new List<AppointmentDto> { dummyDto };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentsQuery>(), default))
            .ReturnsAsync(expectedAppointments);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedAppointments);
    }

    [Fact]
    public async Task GetByPatient_ShouldReturnAppointmentsList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var dummyDto = new AppointmentDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, null, null, null, DentalScheduler.Domain.Enums.AppointmentStatus.Pending, null, null, null, false);
        var expectedAppointments = new List<AppointmentDto> { dummyDto };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPatientAppointmentsQuery>(), default))
            .ReturnsAsync(expectedAppointments);

        // Act
        var result = await _controller.GetByPatient(patientId);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedAppointments);
    }

    [Fact]
    public async Task GetByDoctor_ShouldReturnAppointmentsList()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var dummyDto = new AppointmentDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, null, null, null, DentalScheduler.Domain.Enums.AppointmentStatus.Pending, null, null, null, false);
        var expectedAppointments = new List<AppointmentDto> { dummyDto };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDoctorAppointmentsQuery>(), default))
            .ReturnsAsync(expectedAppointments);

        // Act
        var result = await _controller.GetByDoctor(doctorId, null);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedAppointments);
    }

    [Fact]
    public async Task Create_ShouldReturnOkWithResponse()
    {
        // Arrange
        var command = new CreateAppointmentCommand();
        var expectedResponse = new CreateAppointmentResponse(Guid.NewGuid(), null);
        _mediatorMock.Setup(m => m.Send(command, default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CreateAppointmentResponse>().Subject;
        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UpdateStatus_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = DentalScheduler.Domain.Enums.AppointmentStatus.Completed };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAppointmentStatusCommand>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateStatus(appointmentId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateStatus_WhenFails_ShouldReturnBadRequest()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = DentalScheduler.Domain.Enums.AppointmentStatus.Completed };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAppointmentStatusCommand>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateStatus(appointmentId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetRecommendations_ShouldReturnOkWithResponse()
    {
        // Arrange
        var query = new GetScheduleRecommendationsQuery();
        var expectedResponse = new SchedulingResponseDto();
        _mediatorMock.Setup(m => m.Send(query, default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetRecommendations(query);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SchedulingResponseDto>().Subject;
        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetRecommendations_WhenThrowsException_ShouldReturn500()
    {
        // Arrange
        var query = new GetScheduleRecommendationsQuery();
        _mediatorMock.Setup(m => m.Send(query, default))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var result = await _controller.GetRecommendations(query);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }
}
