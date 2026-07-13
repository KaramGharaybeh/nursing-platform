namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentCompletionDto
{
    public Guid PaymentOrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public IReadOnlyList<Guid> GrantedExamIds { get; set; } = [];
}
