namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
    public int Quantity { get; set; }
    public long LineTotalAmountMinor { get; set; }
}
