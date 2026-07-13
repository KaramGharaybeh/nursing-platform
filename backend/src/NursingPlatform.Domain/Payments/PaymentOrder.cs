using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Payments;

public class PaymentOrder : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid NurseProfileId { get; set; }
    public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.PendingPayment;
    public string Currency { get; set; } = string.Empty;
    public long TotalAmountMinor { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public ICollection<PaymentOrderItem> Items { get; set; } = new List<PaymentOrderItem>();

    public bool IsTerminal => Status is PaymentOrderStatus.Paid
        or PaymentOrderStatus.Failed
        or PaymentOrderStatus.Cancelled
        or PaymentOrderStatus.Expired;

    public static PaymentOrder CreatePending(Guid nurseProfileId, PaymentOrderItem item, DateTime createdAt)
    {
        var order = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            Status = PaymentOrderStatus.PendingPayment,
            Currency = item.Currency,
            TotalAmountMinor = item.LineTotalAmountMinor,
            ExpiresAt = createdAt.AddMinutes(30),
            PaidAt = null,
            CancelledAt = null
        };

        item.OrderId = order.Id;
        order.Items.Add(item);
        return order;
    }

    public void Cancel(DateTime timestamp)
    {
        if (Status != PaymentOrderStatus.PendingPayment)
        {
            throw new InvalidOperationException("Only pending payment orders can be cancelled.");
        }

        Status = PaymentOrderStatus.Cancelled;
        CancelledAt = timestamp;
    }

    public void MarkPaid(DateTime timestamp)
    {
        if (Status != PaymentOrderStatus.PendingPayment)
        {
            throw new InvalidOperationException("Only pending payment orders can be marked paid.");
        }

        Status = PaymentOrderStatus.Paid;
        PaidAt = timestamp;
    }

    public bool ExpireIfPastDue(DateTime timestamp)
    {
        if (Status != PaymentOrderStatus.PendingPayment || ExpiresAt is null || ExpiresAt > timestamp)
        {
            return false;
        }

        Status = PaymentOrderStatus.Expired;
        return true;
    }
}
