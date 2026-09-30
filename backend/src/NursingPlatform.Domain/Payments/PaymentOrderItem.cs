using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Domain.Payments;

public class PaymentOrderItem : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public PaymentProductType ProductTypeSnapshot { get; set; } = PaymentProductType.ExamAccess;
    public Guid ExamIdSnapshot { get; set; }
    public PaymentOrderItemSourceType SourceType { get; set; } = PaymentOrderItemSourceType.ExamAccessProduct;
    public Guid SourceId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long UnitAmountMinor { get; set; }
    public int Quantity { get; set; } = 1;
    public long LineTotalAmountMinor { get; set; }
    public PaymentOrder Order { get; set; } = null!;
    public PaymentProduct? Product { get; set; }
    public PackageOrderItemSnapshot? PackageOrderItemSnapshot { get; set; }

    public static PaymentOrderItem CreateSnapshot(PaymentProduct product)
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProductNameSnapshot = product.Name,
            ProductTypeSnapshot = product.Type,
            ExamIdSnapshot = product.ExamId,
            SourceType = PaymentOrderItemSourceType.ExamAccessProduct,
            SourceId = product.Id,
            Currency = product.Currency,
            UnitAmountMinor = product.UnitAmountMinor,
            Quantity = 1,
            LineTotalAmountMinor = product.UnitAmountMinor
        };
    }

    public static PaymentOrderItem CreatePackageOfferSnapshot(PackageOrderItemSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var item = new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.Empty,
            ProductNameSnapshot = string.Empty,
            ProductTypeSnapshot = PaymentProductType.ExamAccess,
            ExamIdSnapshot = Guid.Empty,
            SourceType = PaymentOrderItemSourceType.PreparationPackageOffer,
            SourceId = snapshot.PackageOfferId,
            Currency = snapshot.Currency,
            UnitAmountMinor = snapshot.PriceAmountMinor,
            Quantity = 1,
            LineTotalAmountMinor = snapshot.PriceAmountMinor,
            PackageOrderItemSnapshot = snapshot
        };

        snapshot.AssignPaymentOrderItem(item.Id);
        return item;
    }
}
