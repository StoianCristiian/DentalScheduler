using DentalScheduler.Domain.Common;

namespace DentalScheduler.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid DentistId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
    public string? TreatmentType { get; set; }
    public Enums.AppointmentStatus Status { get; set; } = Enums.AppointmentStatus.Pending;
}