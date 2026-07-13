namespace NursingPlatform.Application.Payments.Abstractions;

public class CreatePaymentCheckoutProviderSessionResult
{
    public string ProviderCheckoutSessionId { get; set; } = string.Empty;
    public string? ProviderPaymentIntentId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
