using DentalScheduler.Domain.Enums;

namespace DentalScheduler.Api.Controllers;

public class UpdateStatusRequest
{
    public AppointmentStatus Status { get; set; }
    public decimal? Cost { get; set; }
}
