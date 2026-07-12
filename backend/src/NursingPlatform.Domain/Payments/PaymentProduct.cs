using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Domain.Payments;

public class PaymentProduct : AuditableEntity
{
    public Guid Id { get; set; }
    public PaymentProductType Type { get; init; } = PaymentProductType.ExamAccess;
    public Guid ExamId { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
    public bool IsActive { get; set; } = true;
    public Exam Exam { get; set; } = null!;

    public static PaymentProduct CreateExamAccess(
        Guid examId,
        string name,
        string? description,
        string currency,
        long unitAmountMinor,
        bool isActive = true)
    {
        return new PaymentProduct
        {
            Id = Guid.NewGuid(),
            Type = PaymentProductType.ExamAccess,
            ExamId = examId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            UnitAmountMinor = unitAmountMinor,
            IsActive = isActive
        };
    }
}
