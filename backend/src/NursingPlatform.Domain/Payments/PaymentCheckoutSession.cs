using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Domain.Payments;

public class PaymentCheckoutSession : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public Guid NurseProfileId { get; private set; }
    public PaymentCheckoutSessionStatus Status { get; private set; } = PaymentCheckoutSessionStatus.Created;
    public string ProviderName { get; private set; } = string.Empty;
    public string? ProviderCheckoutSessionId { get; private set; }
    public string? ProviderPaymentIntentId { get; private set; }
    public string ProviderClientReference { get; private set; } = string.Empty;
    public string? CheckoutUrl { get; private set; }
    public Guid? ProviderCallLeaseId { get; private set; }
    public DateTime? ProviderCallLeaseExpiresAt { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public long AmountMinor { get; private set; }
    public string? IdempotencyKeyHash { get; private set; }
    public string RequestFingerprintHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public PaymentOrder PaymentOrder { get; set; } = null!;
    public NurseProfile NurseProfile { get; set; } = null!;

    public bool IsTerminal => Status is PaymentCheckoutSessionStatus.CreationRejected
        or PaymentCheckoutSessionStatus.Expired;

    public static PaymentCheckoutSession Create(
        Guid paymentOrderId,
        Guid nurseProfileId,
        string providerName,
        string providerClientReference,
        string currency,
        long amountMinor,
        DateTime orderExpiresAt,
        DateTime requestedExpiresAt,
        string? idempotencyKeyHash,
        string requestFingerprintHash)
    {
        if (paymentOrderId == Guid.Empty)
        {
            throw new InvalidOperationException("Payment order id is required.");
        }

        if (nurseProfileId == Guid.Empty)
        {
            throw new InvalidOperationException("Nurse profile id is required.");
        }

        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new InvalidOperationException("Provider name is required.");
        }

        if (string.IsNullOrWhiteSpace(providerClientReference))
        {
            throw new InvalidOperationException("Provider client reference is required.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        if (amountMinor <= 0)
        {
            throw new InvalidOperationException("Checkout amount must be positive.");
        }

        if (string.IsNullOrWhiteSpace(requestFingerprintHash))
        {
            throw new InvalidOperationException("Request fingerprint hash is required.");
        }

        return new PaymentCheckoutSession
        {
            Id = Guid.NewGuid(),
            PaymentOrderId = paymentOrderId,
            NurseProfileId = nurseProfileId,
            Status = PaymentCheckoutSessionStatus.Created,
            ProviderName = providerName.Trim(),
            ProviderClientReference = providerClientReference.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            AmountMinor = amountMinor,
            IdempotencyKeyHash = string.IsNullOrWhiteSpace(idempotencyKeyHash) ? null : idempotencyKeyHash.Trim(),
            RequestFingerprintHash = requestFingerprintHash.Trim(),
            ExpiresAt = requestedExpiresAt <= orderExpiresAt ? requestedExpiresAt : orderExpiresAt
        };
    }

    public void MarkProviderPending(
        string providerCheckoutSessionId,
        string? providerPaymentIntentId,
        string checkoutUrl,
        DateTime providerExpiresAt)
    {
        if (Status != PaymentCheckoutSessionStatus.Created)
        {
            throw new InvalidOperationException("Only created checkout sessions can become provider pending.");
        }

        if (string.IsNullOrWhiteSpace(providerCheckoutSessionId))
        {
            throw new InvalidOperationException("Provider checkout session id is required.");
        }

        if (!IsHttpsUrl(checkoutUrl))
        {
            throw new InvalidOperationException("Checkout URL must be HTTPS.");
        }

        ProviderCheckoutSessionId = providerCheckoutSessionId.Trim();
        ProviderPaymentIntentId = string.IsNullOrWhiteSpace(providerPaymentIntentId) ? null : providerPaymentIntentId.Trim();
        CheckoutUrl = checkoutUrl.Trim();
        ExpiresAt = providerExpiresAt <= ExpiresAt ? providerExpiresAt : ExpiresAt;
        Status = PaymentCheckoutSessionStatus.ProviderPending;
        ProviderCallLeaseId = null;
        ProviderCallLeaseExpiresAt = null;
    }

    public void MarkCreationRejected()
    {
        if (Status != PaymentCheckoutSessionStatus.Created)
        {
            throw new InvalidOperationException("Only created checkout sessions can be rejected during creation.");
        }

        Status = PaymentCheckoutSessionStatus.CreationRejected;
        ProviderCallLeaseId = null;
        ProviderCallLeaseExpiresAt = null;
    }

    public bool ExpireIfPastDue(DateTime timestamp)
    {
        if (Status is not (PaymentCheckoutSessionStatus.Created or PaymentCheckoutSessionStatus.ProviderPending)
            || ExpiresAt > timestamp)
        {
            return false;
        }

        Status = PaymentCheckoutSessionStatus.Expired;
        ProviderCallLeaseId = null;
        ProviderCallLeaseExpiresAt = null;
        return true;
    }

    public bool IsReusableAt(DateTime timestamp)
    {
        return Status is PaymentCheckoutSessionStatus.Created or PaymentCheckoutSessionStatus.ProviderPending
            && ExpiresAt > timestamp;
    }

    private static bool IsHttpsUrl(string? checkoutUrl)
    {
        return Uri.TryCreate(checkoutUrl, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
    }
}
