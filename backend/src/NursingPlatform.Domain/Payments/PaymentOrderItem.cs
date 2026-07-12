using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Payments;

public class PaymentOrderItem : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public PaymentProductType ProductTypeSnapshot { get; set; } = PaymentProductType.ExamAccess;
    public Guid ExamIdSnapshot { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
    public int Quantity { get; set; } = 1;
    public long LineTotalAmountMinor { get; set; }
    public PaymentOrder Order { get; set; } = null!;
    public PaymentProduct Product { get; set; } = null!;

    public static PaymentOrderItem CreateSnapshot(PaymentProduct product)
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProductNameSnapshot = product.Name,
            ProductTypeSnapshot = product.Type,
            ExamIdSnapshot = product.ExamId,
            Currency = product.Currency,
            UnitAmountMinor = product.UnitAmountMinor,
            Quantity = 1,
            LineTotalAmountMinor = product.UnitAmountMinor
        };
    }
}
