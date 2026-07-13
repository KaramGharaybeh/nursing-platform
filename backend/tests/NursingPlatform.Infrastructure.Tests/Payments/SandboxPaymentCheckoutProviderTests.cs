using Microsoft.Extensions.Options;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Infrastructure.Payments.Sandbox;

namespace NursingPlatform.Infrastructure.Tests.Payments;

public class SandboxPaymentCheckoutProviderTests
{
    [Fact]
    public void ProviderName_IsExactlySandbox()
    {
        var provider = CreateProvider();

        Assert.Equal("Sandbox", provider.ProviderName);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_WithUsd_ReturnsHttpsCheckoutUrlAndOpaqueSessionId()
    {
        var provider = CreateProvider();
        var request = CreateRequest(currency: "USD");

        var result = await provider.CreateCheckoutSessionAsync(request, CancellationToken.None);

        Assert.StartsWith("sandbox_checkout_", result.ProviderCheckoutSessionId, StringComparison.Ordinal);
        Assert.Null(result.ProviderPaymentIntentId);
        Assert.StartsWith("https://sandbox-payments.local/checkout/", result.CheckoutUrl, StringComparison.Ordinal);
        Assert.Equal(request.ExpiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_WithNonUsdCurrency_ThrowsSafeUnavailableException()
    {
        var provider = CreateProvider();

        var exception = await Assert.ThrowsAsync<PaymentCheckoutProviderUnavailableException>(() =>
            provider.CreateCheckoutSessionAsync(CreateRequest(currency: "EUR"), CancellationToken.None));

        Assert.DoesNotContain("sandbox-payments.local", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_WithInvalidPublicBaseUrl_ThrowsSafeUnavailableException()
    {
        var provider = CreateProvider(publicBaseUrl: "http://sandbox-payments.local");

        var exception = await Assert.ThrowsAsync<PaymentCheckoutProviderUnavailableException>(() =>
            provider.CreateCheckoutSessionAsync(CreateRequest(currency: "USD"), CancellationToken.None));

        Assert.DoesNotContain("http://sandbox-payments.local", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static SandboxPaymentCheckoutProvider CreateProvider(
        string publicBaseUrl = "https://sandbox-payments.local",
        string supportedCurrency = "USD")
    {
        return new SandboxPaymentCheckoutProvider(Options.Create(new SandboxPaymentSettings
        {
            PublicBaseUrl = publicBaseUrl,
            SupportedCurrency = supportedCurrency
        }));
    }

    private static CreatePaymentCheckoutProviderSessionRequest CreateRequest(string currency)
    {
        return new CreatePaymentCheckoutProviderSessionRequest
        {
            PaymentOrderId = Guid.NewGuid(),
            CheckoutSessionId = Guid.NewGuid(),
            ProviderClientReference = "checkout_" + Guid.NewGuid().ToString("N"),
            Currency = currency,
            AmountMinor = 1000,
            Description = "Payment order checkout",
            SuccessUrl = "https://nursing-platform.local/payments/checkout/success",
            CancelUrl = "https://nursing-platform.local/payments/checkout/cancel",
            ExpiresAt = DateTime.UtcNow.AddMinutes(20)
        };
    }
}
