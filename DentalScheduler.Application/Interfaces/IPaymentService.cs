namespace DentalScheduler.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creează un PaymentIntent pe Stripe pentru o anumită sumă
    /// </summary>
    Task<string> CreatePaymentIntentAsync(decimal amount, string appointmentId, string description = "");

    /// <summary>
    /// Retrieve PaymentIntent details from Stripe
    /// </summary>
    Task<PaymentIntentDetails?> GetPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Confirmă plata și marchează appointment-ul ca plătit
    /// </summary>
    Task<bool> ConfirmPaymentAsync(string paymentIntentId);
}

public class PaymentIntentDetails
{
    public string Id { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ClientSecret { get; set; }
}
