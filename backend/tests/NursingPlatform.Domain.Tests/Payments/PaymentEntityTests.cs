using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Domain.Tests.Payments;

public class PaymentEntityTests
{
    [Fact]
    public void PaymentProduct_DefaultType_IsExamAccess()
    {
        var product = new PaymentProduct();

        Assert.Equal(PaymentProductType.ExamAccess, product.Type);
    }

    [Fact]
    public void PaymentProduct_DefaultActiveState_IsTrue()
    {
        var product = new PaymentProduct();

        Assert.True(product.IsActive);
    }

    [Fact]
    public void PaymentProduct_CreateExamAccess_NormalizesCurrencyAndUsesMinorUnitAmount()
    {
        var product = PaymentProduct.CreateExamAccess(Guid.NewGuid(), "  NCLEX  ", null, "usd", 4999);

        Assert.Equal(PaymentProductType.ExamAccess, product.Type);
        Assert.Equal("NCLEX", product.Name);
        Assert.Equal("USD", product.Currency);
        Assert.Equal(4999, product.UnitAmountMinor);
    }

    [Fact]
    public void PaymentOrder_CreatePending_SetsPendingPaymentStatusAndTotal()
    {
        var item = CreateItem(2499);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), item, new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Equal(2499, order.TotalAmountMinor);
        Assert.Equal("USD", order.Currency);
        Assert.Null(order.PaidAt);
        Assert.Null(order.CancelledAt);
    }

    [Fact]
    public void PaymentOrder_CreatePending_SetsExpiresAtToCreatedAtPlusThirtyMinutes()
    {
        var createdAt = new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), createdAt);

        Assert.Equal(createdAt.AddMinutes(30), order.ExpiresAt);
    }

    [Fact]
    public void PaymentOrder_CancelPending_SetsCancelledStatusAndTimestamp()
    {
        var timestamp = new DateTime(2026, 7, 12, 11, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), timestamp.AddMinutes(-10));

        order.Cancel(timestamp);

        Assert.Equal(PaymentOrderStatus.Cancelled, order.Status);
        Assert.Equal(timestamp, order.CancelledAt);
    }

    [Fact]
    public void PaymentOrder_CancelTerminalStatus_ThrowsInvalidOperationException()
    {
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), DateTime.UtcNow);
        order.Status = PaymentOrderStatus.Expired;

        Assert.Throws<InvalidOperationException>(() => order.Cancel(DateTime.UtcNow));
    }

    [Fact]
    public void PaymentOrder_ExpirePastDuePending_SetsExpiredStatus()
    {
        var createdAt = new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), createdAt);

        var changed = order.ExpireIfPastDue(createdAt.AddMinutes(31));

        Assert.True(changed);
        Assert.Equal(PaymentOrderStatus.Expired, order.Status);
    }

    [Fact]
    public void PaymentOrderItem_CreateSnapshot_CopiesProductFieldsAndLineTotal()
    {
        var examId = Guid.NewGuid();
        var product = PaymentProduct.CreateExamAccess(examId, "Exam Access", "Description", "usd", 3500);

        var item = PaymentOrderItem.CreateSnapshot(product);

        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal("Exam Access", item.ProductNameSnapshot);
        Assert.Equal(PaymentProductType.ExamAccess, item.ProductTypeSnapshot);
        Assert.Equal(examId, item.ExamIdSnapshot);
        Assert.Equal("USD", item.Currency);
        Assert.Equal(3500, item.UnitAmountMinor);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(3500, item.LineTotalAmountMinor);
    }

    [Fact]
    public void PaymentCheckoutSession_Create_CopiesOrderOwnershipAmountCurrencyAndExpiry()
    {
        var orderId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var orderExpiresAt = createdAt.AddMinutes(30);

        var session = PaymentCheckoutSession.Create(
            orderId,
            nurseProfileId,
            "provider-neutral",
            "checkout-ref-1",
            "USD",
            4999,
            orderExpiresAt,
            createdAt.AddMinutes(20),
            "idem-hash",
            "fingerprint-hash");

        Assert.Equal(orderId, session.PaymentOrderId);
        Assert.Equal(nurseProfileId, session.NurseProfileId);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
        Assert.Equal("provider-neutral", session.ProviderName);
        Assert.Equal("checkout-ref-1", session.ProviderClientReference);
        Assert.Equal("USD", session.Currency);
        Assert.Equal(4999, session.AmountMinor);
        Assert.Equal(createdAt.AddMinutes(20), session.ExpiresAt);
        Assert.Equal("idem-hash", session.IdempotencyKeyHash);
        Assert.Equal("fingerprint-hash", session.RequestFingerprintHash);
    }

    [Fact]
    public void PaymentCheckoutSession_Create_CapsExpiryToOrderExpiry()
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var orderExpiresAt = createdAt.AddMinutes(15);

        var session = PaymentCheckoutSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "provider-neutral",
            "checkout-ref-1",
            "USD",
            4999,
            orderExpiresAt,
            createdAt.AddMinutes(30),
            null,
            "fingerprint-hash");

        Assert.Equal(orderExpiresAt, session.ExpiresAt);
    }

    [Fact]
    public void PaymentCheckoutSession_Created_AllowsNullProviderCheckoutSessionIdAndCheckoutUrl()
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);

        var session = PaymentCheckoutSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "provider-neutral",
            "checkout-ref-1",
            "USD",
            4999,
            createdAt.AddMinutes(30),
            createdAt.AddMinutes(20),
            null,
            "fingerprint-hash");

        Assert.Null(session.ProviderCheckoutSessionId);
        Assert.Null(session.CheckoutUrl);
    }

    [Fact]
    public void PaymentCheckoutSession_MarkProviderPending_StoresSafeProviderIdentifiersAndCheckoutUrl()
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var session = CreateCheckoutSession(createdAt);

        session.MarkProviderPending(
            "provider-session-1",
            "provider-intent-1",
            "https://checkout.example.test/session/token",
            createdAt.AddMinutes(10));

        Assert.Equal(PaymentCheckoutSessionStatus.ProviderPending, session.Status);
        Assert.Equal("provider-session-1", session.ProviderCheckoutSessionId);
        Assert.Equal("provider-intent-1", session.ProviderPaymentIntentId);
        Assert.Equal("https://checkout.example.test/session/token", session.CheckoutUrl);
        Assert.Equal(createdAt.AddMinutes(10), session.ExpiresAt);
    }

    [Theory]
    [InlineData(null, "https://checkout.example.test/session/token")]
    [InlineData("", "https://checkout.example.test/session/token")]
    [InlineData("provider-session-1", null)]
    [InlineData("provider-session-1", "")]
    public void PaymentCheckoutSession_MarkProviderPending_RequiresProviderCheckoutSessionIdAndCheckoutUrl(
        string? providerCheckoutSessionId,
        string? checkoutUrl)
    {
        var session = CreateCheckoutSession(new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc));

        Assert.Throws<InvalidOperationException>(() => session.MarkProviderPending(
            providerCheckoutSessionId!,
            null,
            checkoutUrl!,
            session.ExpiresAt));
    }

    [Theory]
    [InlineData("http://checkout.example.test/session/token")]
    [InlineData("not-a-url")]
    public void PaymentCheckoutSession_MarkProviderPending_RejectsNonHttpsCheckoutUrl(string checkoutUrl)
    {
        var session = CreateCheckoutSession(new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc));

        Assert.Throws<InvalidOperationException>(() => session.MarkProviderPending(
            "provider-session-1",
            null,
            checkoutUrl,
            session.ExpiresAt));
    }

    [Fact]
    public void PaymentCheckoutSession_MarkCreationRejected_SetsTerminalCreationRejectedStatus()
    {
        var session = CreateCheckoutSession(new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc));

        session.MarkCreationRejected();

        Assert.Equal(PaymentCheckoutSessionStatus.CreationRejected, session.Status);
        Assert.True(session.IsTerminal);
        Assert.False(session.IsReusableAt(DateTime.UtcNow));
    }

    [Fact]
    public void PaymentCheckoutSession_MarkCreationRejected_IsNotPaymentFailedState()
    {
        var session = CreateCheckoutSession(new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc));

        session.MarkCreationRejected();

        Assert.DoesNotContain("Failed", session.Status.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaymentCheckoutSession_ExpirePastDueProviderPending_SetsExpiredStatus()
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var session = CreateCheckoutSession(createdAt);
        session.MarkProviderPending(
            "provider-session-1",
            null,
            "https://checkout.example.test/session/token",
            createdAt.AddMinutes(10));

        var changed = session.ExpireIfPastDue(createdAt.AddMinutes(11));

        Assert.True(changed);
        Assert.Equal(PaymentCheckoutSessionStatus.Expired, session.Status);
    }

    [Fact]
    public void PaymentCheckoutSession_LifecycleCriticalProperties_DoNotExposePublicSetters()
    {
        var protectedProperties = new[]
        {
            nameof(PaymentCheckoutSession.Status),
            nameof(PaymentCheckoutSession.PaymentOrderId),
            nameof(PaymentCheckoutSession.NurseProfileId),
            nameof(PaymentCheckoutSession.ProviderName),
            nameof(PaymentCheckoutSession.ProviderCheckoutSessionId),
            nameof(PaymentCheckoutSession.ProviderPaymentIntentId),
            nameof(PaymentCheckoutSession.ProviderClientReference),
            nameof(PaymentCheckoutSession.CheckoutUrl),
            nameof(PaymentCheckoutSession.Currency),
            nameof(PaymentCheckoutSession.AmountMinor),
            nameof(PaymentCheckoutSession.IdempotencyKeyHash),
            nameof(PaymentCheckoutSession.RequestFingerprintHash),
            nameof(PaymentCheckoutSession.ProviderCallLeaseId),
            nameof(PaymentCheckoutSession.ProviderCallLeaseExpiresAt),
            nameof(PaymentCheckoutSession.ExpiresAt)
        };

        foreach (var propertyName in protectedProperties)
        {
            var property = typeof(PaymentCheckoutSession).GetProperty(propertyName)!;

            Assert.NotNull(property.SetMethod);
            Assert.True(property.SetMethod.IsPrivate, $"{propertyName} must use a private setter.");
        }
    }

    [Fact]
    public void PaymentCheckoutSession_CreationRejected_CannotTransitionBackToProviderPending()
    {
        var session = CreateCheckoutSession(new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc));
        session.MarkCreationRejected();

        Assert.Throws<InvalidOperationException>(() => session.MarkProviderPending(
            "provider-session-1",
            null,
            "https://checkout.example.test/session/token",
            session.ExpiresAt));
        Assert.Equal(PaymentCheckoutSessionStatus.CreationRejected, session.Status);
    }

    [Fact]
    public void PaymentCheckoutSession_Expired_CannotTransitionBackToProviderPending()
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var session = CreateCheckoutSession(createdAt);
        session.ExpireIfPastDue(createdAt.AddMinutes(21));

        Assert.Throws<InvalidOperationException>(() => session.MarkProviderPending(
            "provider-session-1",
            null,
            "https://checkout.example.test/session/token",
            session.ExpiresAt));
        Assert.Equal(PaymentCheckoutSessionStatus.Expired, session.Status);
    }

    [Theory]
    [InlineData(PaymentCheckoutSessionStatus.Created, true)]
    [InlineData(PaymentCheckoutSessionStatus.ProviderPending, true)]
    [InlineData(PaymentCheckoutSessionStatus.CreationRejected, false)]
    [InlineData(PaymentCheckoutSessionStatus.Expired, false)]
    public void PaymentCheckoutSession_TerminalStates_AreNotReusable(PaymentCheckoutSessionStatus status, bool expectedReusable)
    {
        var createdAt = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc);
        var session = CreateCheckoutSessionInStatus(status, createdAt);

        Assert.Equal(expectedReusable, session.IsReusableAt(createdAt.AddMinutes(1)));
    }

    private static PaymentOrderItem CreateItem(long amount)
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Exam Access",
            ProductTypeSnapshot = PaymentProductType.ExamAccess,
            ExamIdSnapshot = Guid.NewGuid(),
            Currency = "USD",
            UnitAmountMinor = amount,
            Quantity = 1,
            LineTotalAmountMinor = amount
        };
    }

    private static PaymentCheckoutSession CreateCheckoutSession(DateTime createdAt)
    {
        return PaymentCheckoutSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "provider-neutral",
            "checkout-ref-1",
            "USD",
            4999,
            createdAt.AddMinutes(30),
            createdAt.AddMinutes(20),
            null,
            "fingerprint-hash");
    }

    private static PaymentCheckoutSession CreateCheckoutSessionInStatus(PaymentCheckoutSessionStatus status, DateTime createdAt)
    {
        var session = CreateCheckoutSession(createdAt);

        switch (status)
        {
            case PaymentCheckoutSessionStatus.Created:
                break;
            case PaymentCheckoutSessionStatus.ProviderPending:
                session.MarkProviderPending(
                    "provider-session-1",
                    null,
                    "https://checkout.example.test/session/token",
                    createdAt.AddMinutes(10));
                break;
            case PaymentCheckoutSessionStatus.CreationRejected:
                session.MarkCreationRejected();
                break;
            case PaymentCheckoutSessionStatus.Expired:
                session.ExpireIfPastDue(createdAt.AddMinutes(21));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return session;
    }
}
