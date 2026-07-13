namespace NursingPlatform.Application.Payments.Abstractions;

public class PaymentCheckoutCreationRejectedException : Exception
{
    public PaymentCheckoutCreationRejectedException()
        : base("Payment checkout creation was definitively rejected.")
    {
    }
}
