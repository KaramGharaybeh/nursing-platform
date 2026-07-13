namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentCheckoutSessionDto
{
    public Guid Id { get; set; }
    public Guid PaymentOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? CheckoutUrl { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long AmountMinor { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
