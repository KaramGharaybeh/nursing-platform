using Microsoft.Extensions.Options;
using NursingPlatform.Application.Payments.Abstractions;

namespace NursingPlatform.Infrastructure.Payments.Sandbox;

public class SandboxPaymentCheckoutProvider : IPaymentCheckoutProvider
{
    public const string Name = "Sandbox";

    private readonly SandboxPaymentSettings _settings;

    public SandboxPaymentCheckoutProvider(IOptions<SandboxPaymentSettings> settings)
    {
        _settings = settings.Value;
    }

    public string ProviderName => Name;

    public Task<CreatePaymentCheckoutProviderSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentCheckoutProviderSessionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateSettings();

        if (!string.Equals(request.Currency, _settings.SupportedCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentCheckoutProviderUnavailableException("Sandbox payment checkout supports USD only.");
        }

        var sessionId = "sandbox_checkout_" + Guid.NewGuid().ToString("N");
        var checkoutUrl = new Uri(new Uri(_settings.PublicBaseUrl, UriKind.Absolute), $"checkout/{sessionId}");

        return Task.FromResult(new CreatePaymentCheckoutProviderSessionResult
        {
            ProviderCheckoutSessionId = sessionId,
            ProviderPaymentIntentId = null,
            CheckoutUrl = checkoutUrl.ToString(),
            ExpiresAt = request.ExpiresAt
        });
    }

    private void ValidateSettings()
    {
        if (!string.Equals(_settings.SupportedCurrency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentCheckoutProviderUnavailableException("Sandbox payment checkout supports USD only.");
        }

        if (!Uri.TryCreate(_settings.PublicBaseUrl, UriKind.Absolute, out var publicBaseUrl) ||
            !string.Equals(publicBaseUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentCheckoutProviderUnavailableException("Sandbox payment checkout public base URL must be configured as HTTPS.");
        }
    }
}
