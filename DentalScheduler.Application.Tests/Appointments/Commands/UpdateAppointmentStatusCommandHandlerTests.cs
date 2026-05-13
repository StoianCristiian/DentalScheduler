using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.Appointments.Commands.UpdateAppointmentStatus;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using FluentAssertions;
using System.Reflection;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace DentalScheduler.Application.Tests.Appointments.Commands;

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
    public async Task Handle_AppointmentFound_UpdatesStatusAndCost()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment { Id = appointmentId };
        var statusProp = typeof(Appointment).GetProperty(nameof(Appointment.Status));
        var setMethod = statusProp?.GetSetMethod(true);
        setMethod?.Invoke(appointment, new object[] { AppointmentStatus.Pending });
        
        var appointments = new List<Appointment> { appointment };
        _contextMock.Setup(c => c.Appointments).ReturnsDbSet(appointments);

        var command = new UpdateAppointmentStatusCommand
        {
            AppointmentId = appointmentId,
            Status = AppointmentStatus.Completed,
            Cost = 150m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.Cost.Should().Be(150m);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsFalse()
    {
        // Arrange
        _contextMock.Setup(c => c.Appointments).ReturnsDbSet(new List<Appointment>());

        var command = new UpdateAppointmentStatusCommand
        {
            AppointmentId = Guid.NewGuid(),
            Status = AppointmentStatus.Completed
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
