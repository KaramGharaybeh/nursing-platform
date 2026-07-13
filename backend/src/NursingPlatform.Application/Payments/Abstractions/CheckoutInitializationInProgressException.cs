namespace NursingPlatform.Application.Payments.Abstractions;

public class CheckoutInitializationInProgressException : Exception
{
    public CheckoutInitializationInProgressException(TimeSpan retryAfter)
        : base("Checkout initialization is already in progress.")
    {
        RetryAfter = retryAfter < TimeSpan.Zero ? TimeSpan.Zero : retryAfter;
    }

    public TimeSpan RetryAfter { get; }
}
