namespace DentalScheduler.Domain.Enums;

public enum AppointmentStatus
{
    Pending = 0,           // Cerere inițială de la pacient/doctor
    Accepted = 1,          // Doctor a acceptat și a setat prețul
    Rejected = 2,          // Doctor a respins
    Cancelled = 3,         // Anulat
    Confirmed = 4,         // Pacient a confirmat prezența și metoda de plată
    Completed = 5          // Programare finalizată și plătită
}
