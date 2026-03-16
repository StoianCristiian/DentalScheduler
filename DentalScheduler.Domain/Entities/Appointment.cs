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
    public decimal? Cost { get; private set; }
    public Enums.AppointmentStatus Status { get; private set; } = Enums.AppointmentStatus.Pending;

    public void Confirm()
    {
        if (Status == Enums.AppointmentStatus.Completed || Status == Enums.AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Nu poți confirma o programare finalizată sau anulată.");
            
        Status = Enums.AppointmentStatus.Confirmed;
    }

    public void Reject()
    {
        if (Status == Enums.AppointmentStatus.Completed)
            throw new InvalidOperationException("Nu poți respinge o programare deja finalizată.");

        Status = Enums.AppointmentStatus.Rejected;
    }

    public void Complete(decimal cost)
    {
        if (Status != Enums.AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Doar programările confirmate pot fi finalizate.");
            
        if (cost < 0)
            throw new ArgumentException("Costul nu poate fi negativ.", nameof(cost));

        Status = Enums.AppointmentStatus.Completed;
        Cost = cost;
    }
    
    // Pentru EF Core sau inițializare
    public void SetStatus(Enums.AppointmentStatus status) 
    {
        Status = status;
    }
    
    public void SetCost(decimal? cost)
    {
        Cost = cost;
    }
}