namespace NursingPlatform.Application.Payments.Abstractions;

public class CreatePaymentCheckoutProviderSessionRequest
{
    public Guid PaymentOrderId { get; set; }
    public Guid CheckoutSessionId { get; set; }
    public string ProviderClientReference { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public long AmountMinor { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
