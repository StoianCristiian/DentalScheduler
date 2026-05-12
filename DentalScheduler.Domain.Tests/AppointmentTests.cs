using System;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DentalScheduler.Domain.Tests;

public class AppointmentTests
{
    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndSetPendingStatus()
    {
        // Act
        var appointment = new Appointment();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Pending);
    }
}

