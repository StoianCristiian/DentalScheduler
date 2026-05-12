using System;
using DentalScheduler.Application.Appointments.Queries.GetAppointments;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DentalScheduler.Application.Tests.Appointments.Queries;

public class AppointmentDtoTests
{
    [Fact]
    public void ToDto_ShouldMapAppointmentToAppointmentDto()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Notes = "Test note",
            TreatmentType = "Consultation",
            Cost = 150m,
            IsPaid = true
        };
        appointment.SetStatus(AppointmentStatus.Confirmed);

        // Act
        var dto = appointment.ToDto("John Doe", "Dr. Smith", "url");

        // Assert
        dto.Id.Should().Be(appointment.Id);
        dto.PatientId.Should().Be(appointment.PatientId);
        dto.DentistId.Should().Be(appointment.DentistId);
        dto.StartAt.Should().Be(appointment.StartAt);
        dto.EndAt.Should().Be(appointment.EndAt);
        dto.Notes.Should().Be(appointment.Notes);
        dto.TreatmentType.Should().Be(appointment.TreatmentType);
        dto.Cost.Should().Be(appointment.Cost);
        dto.Status.Should().Be(AppointmentStatus.Confirmed);
        dto.PatientName.Should().Be("John Doe");
        dto.DentistName.Should().Be("Dr. Smith");
        dto.DentistProfilePictureUrl.Should().Be("url");
        dto.IsPaid.Should().Be(appointment.IsPaid);
    }
}
