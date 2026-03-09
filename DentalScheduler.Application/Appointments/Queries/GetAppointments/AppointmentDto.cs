using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DentistId,
    DateTime StartAt,
    DateTime EndAt,
    string? Notes,
    string? TreatmentType,
    AppointmentStatus Status,
    string? PatientName,
    string? DentistName
);

public static class AppointmentExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment, string? patientName = null, string? dentistName = null)
        => new(
            appointment.Id,
            appointment.PatientId,
            appointment.DentistId,
            appointment.StartAt,
            appointment.EndAt,
            appointment.Notes,
            appointment.TreatmentType,
            appointment.Status,
            patientName,
            dentistName
        );
}
