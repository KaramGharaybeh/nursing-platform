namespace NursingPlatform.Domain.Payments;

public enum PaymentCheckoutSessionStatus
{
    Created,
    ProviderPending,
    CreationRejected,
    Expired
}
