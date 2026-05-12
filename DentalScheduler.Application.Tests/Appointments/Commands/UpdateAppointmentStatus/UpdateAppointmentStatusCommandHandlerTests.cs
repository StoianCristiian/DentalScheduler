using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.Appointments.Commands.UpdateAppointmentStatus;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace DentalScheduler.Application.Tests.Appointments.Commands.UpdateAppointmentStatus;

public class UpdateAppointmentStatusCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateAppointmentStatusCommandHandler _handler;

    public UpdateAppointmentStatusCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateAppointmentStatusCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ShouldReturnFalse()
    {
        // Arrange
        var command = new UpdateAppointmentStatusCommand { AppointmentId = Guid.NewGuid(), Status = AppointmentStatus.Completed };
        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(new List<Appointment>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAppointmentExists_ShouldUpdateStatusAndReturnTrue()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new UpdateAppointmentStatusCommand { AppointmentId = appointmentId, Status = AppointmentStatus.Completed };
        
        var appointment = new Appointment { Id = appointmentId };
        appointment.SetStatus(AppointmentStatus.Confirmed);
        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(new List<Appointment> { appointment });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCost_ShouldUpdateStatusAndCost()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new UpdateAppointmentStatusCommand { AppointmentId = appointmentId, Status = AppointmentStatus.Completed, Cost = 200m };
        
        var appointment = new Appointment { Id = appointmentId, Cost = 100m };
        appointment.SetStatus(AppointmentStatus.Confirmed);
        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(new List<Appointment> { appointment });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.Cost.Should().Be(200m);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
