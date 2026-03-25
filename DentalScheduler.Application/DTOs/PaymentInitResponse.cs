namespace DentalScheduler.Application.DTOs;

public record PaymentInitResponse(string PublishableKey, string ClientSecret, string StripePaymentIntentId);

