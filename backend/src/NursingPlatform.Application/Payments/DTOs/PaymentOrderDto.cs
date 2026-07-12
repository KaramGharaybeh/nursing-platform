namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentOrderDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public long TotalAmountMinor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<PaymentOrderItemDto> Items { get; set; } = [];
}
