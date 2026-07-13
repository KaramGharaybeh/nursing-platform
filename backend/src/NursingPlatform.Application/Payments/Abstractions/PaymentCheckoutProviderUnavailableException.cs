namespace NursingPlatform.Application.Payments.Abstractions;

public class PaymentCheckoutProviderUnavailableException : Exception
{
    public PaymentCheckoutProviderUnavailableException(string message)
        : base(message)
    {
    }
}
