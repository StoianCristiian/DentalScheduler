namespace DentalScheduler.Client.Models;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DentistId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
    public string? TreatmentType { get; set; }
    public decimal? Cost { get; set; }
    public int Status { get; set; }
    public string? PatientName { get; set; }
    public string? DentistName { get; set; }
}

