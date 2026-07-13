namespace NursingPlatform.Application.Payments.Abstractions;

public interface IPaymentCheckoutProvider
{
    string ProviderName { get; }

    Task<CreatePaymentCheckoutProviderSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentCheckoutProviderSessionRequest request,
        CancellationToken cancellationToken);
}
