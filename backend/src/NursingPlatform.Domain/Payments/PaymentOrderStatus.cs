namespace NursingPlatform.Domain.Payments;

public enum PaymentOrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    Expired = 5
}
