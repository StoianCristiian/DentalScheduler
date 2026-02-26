using DentalScheduler.Domain.Entities;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DentistId,
    DateTime StartAt,
    DateTime EndAt,
    string? Notes
);

public static class AppointmentExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment)
    {
        return new AppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DentistId,
            appointment.StartAt,
            appointment.EndAt,
            appointment.Notes
        );
    }
}
