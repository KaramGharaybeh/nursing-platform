using Microsoft.EntityFrameworkCore;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CancelMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;
using NursingPlatform.Application.Payments.Queries.ListMyPaymentOrders;
using NursingPlatform.Application.Payments.Queries.ListPaymentProducts;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Payments;

public class PaymentHandlerTests
{
    [Fact]
    public async Task Handle_ListProducts_ReturnsOnlyActiveProductsLinkedToPublishedExams()
    {
        await using var context = CreateContext();
        var publishedExam = CreateExam(ExamStatus.Published);
        var draftExam = CreateExam(ExamStatus.Draft);
        var activeProduct = PaymentProduct.CreateExamAccess(publishedExam.Id, "Active", null, "USD", 1000);
        var inactiveProduct = PaymentProduct.CreateExamAccess(publishedExam.Id, "Inactive", null, "USD", 1000, isActive: false);
        var draftProduct = PaymentProduct.CreateExamAccess(draftExam.Id, "Draft", null, "USD", 1000);
        context.Exams.AddRange(publishedExam, draftExam);
        context.PaymentProducts.AddRange(activeProduct, inactiveProduct, draftProduct);
        await context.SaveChangesAsync();
        var handler = new ListPaymentProductsQueryHandler(context);

        var result = await handler.Handle(new ListPaymentProductsQuery(), default);

        var item = Assert.Single(result.Items);
        Assert.Equal(activeProduct.Id, item.Id);
    }

    [Fact]
    public async Task Handle_AdminUpdateProduct_DoesNotMutateExistingOrderItemSnapshots()
    {
        await using var context = CreateContext();
        var exam = CreateExam(ExamStatus.Published);
        var product = PaymentProduct.CreateExamAccess(exam.Id, "Original", null, "USD", 1000);
        var item = PaymentOrderItem.CreateSnapshot(product);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), item, DateTime.UtcNow);
        context.Exams.Add(exam);
        context.PaymentProducts.Add(product);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(item);
        await context.SaveChangesAsync();
        var handler = new UpdateAdminPaymentProductCommandHandler(context);

        await handler.Handle(new UpdateAdminPaymentProductCommand
        {
            Id = product.Id,
            Request = new UpdateAdminPaymentProductRequest
            {
                Name = "Updated",
                Currency = "EUR",
                UnitAmountMinor = 2000
            }
        }, default);

        var snapshot = await context.PaymentOrderItems.SingleAsync();
        Assert.Equal("Original", snapshot.ProductNameSnapshot);
        Assert.Equal("USD", snapshot.Currency);
        Assert.Equal(1000, snapshot.UnitAmountMinor);
        Assert.Equal(1000, snapshot.LineTotalAmountMinor);
        Assert.True(product.IsActive);
        Assert.Equal(PaymentProductType.ExamAccess, product.Type);
        Assert.Equal(exam.Id, product.ExamId);
    }

    [Fact]
    public async Task Handle_CreateOrder_CreatesPendingPaymentOrderWithPriceSnapshotAndNoGrant()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var exam = CreateExam(ExamStatus.Published);
        var product = PaymentProduct.CreateExamAccess(exam.Id, "Exam Access", "Description", "usd", 4999);
        context.Exams.Add(exam);
        context.PaymentProducts.Add(product);
        await context.SaveChangesAsync();
        var handler = new CreateMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        var before = DateTime.UtcNow;
        var result = await handler.Handle(new CreateMyPaymentOrderCommand
        {
            Request = new CreatePaymentOrderRequest { ProductId = product.Id }
        }, default);
        var after = DateTime.UtcNow;

        Assert.Equal("PendingPayment", result.Status);
        Assert.Equal(4999, result.TotalAmountMinor);
        var item = Assert.Single(result.Items);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(4999, item.LineTotalAmountMinor);
        Assert.Equal(product.Id, item.ProductId);
        Assert.InRange(result.ExpiresAt!.Value, before.AddMinutes(30).AddSeconds(-1), after.AddMinutes(30).AddSeconds(1));
        Assert.Empty(context.ExamAccessGrants);
    }

    [Fact]
    public async Task Handle_ListOrders_LazilyExpiresPastDuePendingOrders()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow.AddHours(-1));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new ListMyPaymentOrdersQueryHandler(context, CreateGuard(context, userId));

        var result = await handler.Handle(new ListMyPaymentOrdersQuery(), default);

        Assert.Equal("Expired", Assert.Single(result.Items).Status);
        Assert.Equal(PaymentOrderStatus.Expired, order.Status);
    }

    [Fact]
    public async Task Handle_CancelOrder_WhenPastDuePending_ExpiresFirstAndThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow.AddHours(-1));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new CancelMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CancelMyPaymentOrderCommand { Id = order.Id }, default));

        Assert.Equal(PaymentOrderStatus.Expired, order.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_CreatesSuccessfulProviderPendingCheckoutFromOrderSnapshotTotals()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem("EUR", 2500), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal(order.Id, result.PaymentOrderId);
        Assert.Equal("ProviderPending", result.Status);
        Assert.Equal(provider.ProviderName, result.ProviderName);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(2500, result.AmountMinor);
        Assert.Equal("https://checkout.test/session", result.CheckoutUrl);
        Assert.Equal(1, provider.CallCount);
        Assert.Equal(order.Id, provider.LastRequest!.PaymentOrderId);
        Assert.Equal("EUR", provider.LastRequest.Currency);
        Assert.Equal(2500, provider.LastRequest.AmountMinor);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Empty(context.ExamAccessGrants);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithExistingProviderPendingSession_ReusesActiveSessionWithoutCallingProvider()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        existing.MarkProviderPending("provider_existing", null, "https://checkout.test/existing", DateTime.UtcNow.AddMinutes(10));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("ProviderPending", result.Status);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithExistingProviderPendingSession_ReusesActiveSessionWithoutConfiguredProvider()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        existing.MarkProviderPending("provider_existing", null, "https://checkout.test/existing", DateTime.UtcNow.AddMinutes(10));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), []);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("ProviderPending", result.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithNoConfiguredProviderForNewSession_FailsBeforeSessionOrLeaseCreation()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), []);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Contains("provider", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(context.PaymentCheckoutSessions);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithMultipleConfiguredProvidersForNewSession_FailsBeforeSessionOrProviderCall()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var firstProvider = new FakeCheckoutProvider { ProviderName = "FirstProvider" };
        var secondProvider = new FakeCheckoutProvider { ProviderName = "SecondProvider" };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [firstProvider, secondProvider]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Contains("Multiple", exception.Message, StringComparison.Ordinal);
        Assert.Empty(context.PaymentCheckoutSessions);
        Assert.Equal(0, firstProvider.CallCount);
        Assert.Equal(0, secondProvider.CallCount);
    }

    [Fact]
    public async Task Handle_StartCheckout_ExpiresStaleCreatedAndProviderPendingSessionsBeforeNewInsert()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var staleCreated = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow.AddHours(-2));
        var stalePending = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow.AddHours(-2));
        stalePending.MarkProviderPending("provider_stale", null, "https://checkout.test/stale", DateTime.UtcNow.AddHours(-1));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.AddRange(staleCreated, stalePending);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal(PaymentCheckoutSessionStatus.Expired, staleCreated.Status);
        Assert.Equal(PaymentCheckoutSessionStatus.Expired, stalePending.Status);
        Assert.NotEqual(staleCreated.Id, result.Id);
        Assert.NotEqual(stalePending.Id, result.Id);
        Assert.Equal(1, context.PaymentCheckoutSessions.Count(s => s.Status == PaymentCheckoutSessionStatus.ProviderPending));
    }

    [Fact]
    public async Task Handle_StartCheckout_WithSameIdempotencyKeySameFingerprint_ReusesExistingActiveSession()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var idempotencyKey = "same-key";
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(
            order,
            nurseProfileId,
            DateTime.UtcNow,
            StartMyPaymentCheckoutCommandHandler.HashIdempotencyKey(idempotencyKey),
            StartMyPaymentCheckoutCommandHandler.ComputeRequestFingerprintHash(nurseProfileId, order.Id));
        existing.MarkProviderPending("provider_existing", null, "https://checkout.test/existing", DateTime.UtcNow.AddMinutes(10));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand
        {
            OrderId = order.Id,
            Request = new StartPaymentCheckoutRequest { IdempotencyKey = idempotencyKey }
        }, default);

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithIdempotencyKeyDifferentFingerprint_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var idempotencyKey = "same-key-different-fingerprint";
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(
            order,
            nurseProfileId,
            DateTime.UtcNow,
            StartMyPaymentCheckoutCommandHandler.HashIdempotencyKey(idempotencyKey),
            "fp:v1:different");
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new StartMyPaymentCheckoutCommand
        {
            OrderId = order.Id,
            Request = new StartPaymentCheckoutRequest { IdempotencyKey = idempotencyKey }
        }, default));
    }

    [Fact]
    public async Task Handle_StartCheckout_WithSameIdempotencyKeyDifferentOrder_ThrowsConflictWithoutCallingProvider()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var idempotencyKey = "same-key-different-order";
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var firstOrder = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var secondOrder = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(
            firstOrder,
            nurseProfileId,
            DateTime.UtcNow,
            StartMyPaymentCheckoutCommandHandler.HashIdempotencyKey(idempotencyKey),
            StartMyPaymentCheckoutCommandHandler.ComputeRequestFingerprintHash(nurseProfileId, firstOrder.Id));
        context.PaymentOrders.AddRange(firstOrder, secondOrder);
        context.PaymentOrderItems.AddRange(firstOrder.Items.Single(), secondOrder.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new StartMyPaymentCheckoutCommand
        {
            OrderId = secondOrder.Id,
            Request = new StartPaymentCheckoutRequest { IdempotencyKey = idempotencyKey }
        }, default));

        Assert.Equal(0, provider.CallCount);
    }

    [Theory]
    [InlineData(PaymentCheckoutSessionStatus.Expired)]
    [InlineData(PaymentCheckoutSessionStatus.CreationRejected)]
    public async Task Handle_StartCheckout_WithTerminalIdempotencyKeyReuse_ThrowsIdempotencyKeyAlreadyUsed(PaymentCheckoutSessionStatus terminalStatus)
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var idempotencyKey = $"terminal-{terminalStatus}";
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var existing = CreateCheckoutSession(
            order,
            nurseProfileId,
            DateTime.UtcNow,
            StartMyPaymentCheckoutCommandHandler.HashIdempotencyKey(idempotencyKey),
            StartMyPaymentCheckoutCommandHandler.ComputeRequestFingerprintHash(nurseProfileId, order.Id));
        if (terminalStatus == PaymentCheckoutSessionStatus.CreationRejected)
        {
            existing.MarkCreationRejected();
        }
        else
        {
            Assert.True(existing.ExpireIfPastDue(DateTime.UtcNow.AddHours(1)));
        }

        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(existing);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new StartMyPaymentCheckoutCommand
        {
            OrderId = order.Id,
            Request = new StartPaymentCheckoutRequest { IdempotencyKey = idempotencyKey }
        }, default));
        Assert.Contains("IdempotencyKeyAlreadyUsed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithConcurrentActiveLease_ReturnsCheckoutInitializationInProgressWithRetryInformation()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        var leaseId = Guid.NewGuid();
        var leaseExpiresAt = DateTime.UtcNow.AddSeconds(30);
        Assert.True(session.TryAcquireProviderCallLease(leaseId, leaseExpiresAt, DateTime.UtcNow));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var exception = await Assert.ThrowsAsync<CheckoutInitializationInProgressException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.InRange(exception.RetryAfter, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        Assert.Empty(exception.Data);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(leaseId, session.ProviderCallLeaseId);
    }

    [Fact]
    public async Task Handle_StartCheckout_WithConcurrentActiveLease_DoesNotRequireConfiguredProvider()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        var leaseId = Guid.NewGuid();
        Assert.True(session.TryAcquireProviderCallLease(leaseId, DateTime.UtcNow.AddSeconds(30), DateTime.UtcNow));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), []);

        await Assert.ThrowsAsync<CheckoutInitializationInProgressException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Equal(leaseId, session.ProviderCallLeaseId);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_AfterUnknownProviderOutcome_ImmediateDuplicateReturnsInitializationInProgressWithoutSecondProviderCall()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider { ThrowTimeout = true };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        await Assert.ThrowsAsync<CheckoutInitializationInProgressException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Equal(1, provider.CallCount);
        var session = Assert.Single(context.PaymentCheckoutSessions);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
        Assert.NotNull(session.ProviderCallLeaseId);
        Assert.True(session.ProviderCallLeaseExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_StartCheckout_WhenCallerCancellationOccursDuringProviderCall_PropagatesOperationCanceledException()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        using var cancellationTokenSource = new CancellationTokenSource();
        var provider = new FakeCheckoutProvider { CancelCallerDuringProviderCall = cancellationTokenSource };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, cancellationTokenSource.Token));

        Assert.Equal(1, provider.CallCount);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, Assert.Single(context.PaymentCheckoutSessions).Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_DeterministicUniqueInsertRace_ReusesWinningProviderPendingSessionWithoutProviderCall()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        context.SimulateCheckoutInsertUniqueRace = true;
        context.WinningCheckoutSessionFactory = attemptedSession =>
        {
            var winning = PaymentCheckoutSession.Create(
                attemptedSession.PaymentOrderId,
                attemptedSession.NurseProfileId,
                attemptedSession.ProviderName,
                $"checkout_winner_{Guid.NewGuid():N}",
                attemptedSession.Currency,
                attemptedSession.AmountMinor,
                attemptedSession.ExpiresAt,
                attemptedSession.ExpiresAt,
                attemptedSession.IdempotencyKeyHash,
                attemptedSession.RequestFingerprintHash);
            winning.MarkProviderPending("provider_winner", null, "https://checkout.test/winner", DateTime.UtcNow.AddMinutes(10));
            return winning;
        };
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal("ProviderPending", result.Status);
        Assert.Equal("https://checkout.test/winner", result.CheckoutUrl);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(1, context.PaymentCheckoutSessions.Count(s => s.Status == PaymentCheckoutSessionStatus.ProviderPending));
        Assert.Equal(1, context.PaymentCheckoutSessions.Count(s => s.PaymentOrderId == order.Id));
    }

    [Fact]
    public async Task Handle_StartCheckout_WithExpiredLease_RecoversUsingStableProviderClientReference()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        var originalReference = session.ProviderClientReference;
        Assert.True(session.TryAcquireProviderCallLease(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(-2)));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        var result = await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default);

        Assert.Equal(session.Id, result.Id);
        Assert.Equal("ProviderPending", result.Status);
        Assert.Equal(originalReference, provider.LastRequest!.ProviderClientReference);
        Assert.Null(session.ProviderCallLeaseId);
        Assert.Null(session.ProviderCallLeaseExpiresAt);
    }

    [Fact]
    public async Task Handle_StartCheckout_DefinitiveProviderRejection_MarksCreationRejectedWithoutFailingOrder()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider { DefinitivelyReject = true };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Equal(PaymentCheckoutSessionStatus.CreationRejected, Assert.Single(context.PaymentCheckoutSessions).Status);
        Assert.Null(Assert.Single(context.PaymentCheckoutSessions).ProviderCallLeaseId);
        Assert.Null(Assert.Single(context.PaymentCheckoutSessions).ProviderCallLeaseExpiresAt);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_DefinitiveProviderRejectionByNonOwner_DoesNotRejectSession()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider
        {
            DefinitivelyReject = true,
            BeforeThrow = () => ForceCheckoutLeaseOwner(context.PaymentCheckoutSessions.Single(), Guid.NewGuid(), DateTime.UtcNow.AddSeconds(30))
        };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        var session = Assert.Single(context.PaymentCheckoutSessions);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
        Assert.NotNull(session.ProviderCallLeaseId);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_TimeoutOrUnknownOutcome_RemainsRecoverableCreated()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider { ThrowTimeout = true };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        var session = Assert.Single(context.PaymentCheckoutSessions);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
        Assert.NotNull(session.ProviderCallLeaseId);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
    }

    [Fact]
    public async Task Handle_StartCheckout_WhenOrderNotOwned_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));
    }

    [Fact]
    public async Task Handle_StartCheckout_WhenOrderMissing_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Handle_StartCheckout_WhenOrderStatusInvalid_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        order.Cancel(DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));
    }

    [Fact]
    public async Task Handle_StartCheckout_RequestFingerprintExcludesRawIdempotencyKey()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var idempotencyKey = "raw-key-that-must-not-appear";
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [new FakeCheckoutProvider()]);

        await handler.Handle(new StartMyPaymentCheckoutCommand
        {
            OrderId = order.Id,
            Request = new StartPaymentCheckoutRequest { IdempotencyKey = idempotencyKey }
        }, default);

        var session = Assert.Single(context.PaymentCheckoutSessions);
        Assert.DoesNotContain(idempotencyKey, session.RequestFingerprintHash, StringComparison.Ordinal);
        Assert.NotEqual(idempotencyKey, session.IdempotencyKeyHash);
    }

    [Fact]
    public async Task Handle_StartCheckout_WhenProviderReturnsNonHttpsCheckoutUrl_DoesNotTransitionProviderPending()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        await context.SaveChangesAsync();
        var provider = new FakeCheckoutProvider { CheckoutUrl = "http://checkout.test/session" };
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = order.Id }, default));

        Assert.Equal(PaymentCheckoutSessionStatus.Created, Assert.Single(context.PaymentCheckoutSessions).Status);
    }

    [Fact]
    public async Task Handle_CancelOrder_WithActiveCreatedCheckoutSession_ReturnsCheckoutInProgressWithoutMutation()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var handler = new CancelMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CancelMyPaymentOrderCommand { Id = order.Id }, default));

        Assert.Contains("CheckoutInProgress", exception.Message, StringComparison.Ordinal);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Null(order.CancelledAt);
        Assert.Equal(PaymentCheckoutSessionStatus.Created, session.Status);
        Assert.Null(session.ProviderCallLeaseId);
    }

    [Fact]
    public async Task Handle_CancelOrder_WithActiveProviderPendingCheckoutSession_ReturnsCheckoutInProgressWithoutMutation()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        session.MarkProviderPending("provider_pending", null, "https://checkout.test/pending", DateTime.UtcNow.AddMinutes(10));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var handler = new CancelMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CancelMyPaymentOrderCommand { Id = order.Id }, default));

        Assert.Contains("CheckoutInProgress", exception.Message, StringComparison.Ordinal);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Null(order.CancelledAt);
        Assert.Equal(PaymentCheckoutSessionStatus.ProviderPending, session.Status);
        Assert.Equal("https://checkout.test/pending", session.CheckoutUrl);
    }

    [Fact]
    public async Task Handle_CancelOrder_WithStaleCreatedAndProviderPendingCheckoutSessions_ExpiresThemTransactionallyAndCancelsOrder()
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var staleCreated = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow.AddHours(-2));
        var stalePending = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow.AddHours(-2));
        stalePending.MarkProviderPending("provider_stale", null, "https://checkout.test/stale", DateTime.UtcNow.AddHours(-1));
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.AddRange(staleCreated, stalePending);
        await context.SaveChangesAsync();
        var handler = new CancelMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        var result = await handler.Handle(new CancelMyPaymentOrderCommand { Id = order.Id }, default);

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(PaymentOrderStatus.Cancelled, order.Status);
        Assert.Equal(PaymentCheckoutSessionStatus.Expired, staleCreated.Status);
        Assert.Equal(PaymentCheckoutSessionStatus.Expired, stalePending.Status);
    }

    [Theory]
    [InlineData(PaymentCheckoutSessionStatus.Expired)]
    [InlineData(PaymentCheckoutSessionStatus.CreationRejected)]
    public async Task Handle_CancelOrder_WithTerminalCheckoutSession_AllowsCancellation(PaymentCheckoutSessionStatus terminalStatus)
    {
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        await using var context = CreateContext();
        SeedNurse(context, userId, nurseProfileId);
        var order = PaymentOrder.CreatePending(nurseProfileId, CreateItem(), DateTime.UtcNow);
        var session = CreateCheckoutSession(order, nurseProfileId, DateTime.UtcNow);
        if (terminalStatus == PaymentCheckoutSessionStatus.CreationRejected)
        {
            session.MarkCreationRejected();
        }
        else
        {
            Assert.True(session.ExpireIfPastDue(DateTime.UtcNow.AddHours(1)));
        }

        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(order.Items.Single());
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        var handler = new CancelMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));

        var result = await handler.Handle(new CancelMyPaymentOrderCommand { Id = order.Id }, default);

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(PaymentOrderStatus.Cancelled, order.Status);
        Assert.Equal(terminalStatus, session.Status);
    }

    [Fact]
    public void CancelOrderHandler_DoesNotDependOnPaymentCheckoutProvider()
    {
        var constructor = typeof(CancelMyPaymentOrderCommandHandler).GetConstructors().Single();

        Assert.DoesNotContain(constructor.GetParameters(), p => p.ParameterType == typeof(IPaymentCheckoutProvider));
    }

    private static TestPaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestPaymentDbContext(options);
    }

    private static Exam CreateExam(ExamStatus status)
    {
        return new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = Guid.NewGuid(),
            Title = $"Exam {Guid.NewGuid()}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = status
        };
    }

    private static PaymentOrderItem CreateItem(string currency = "USD", long amountMinor = 1000)
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Exam Access",
            ProductTypeSnapshot = PaymentProductType.ExamAccess,
            ExamIdSnapshot = Guid.NewGuid(),
            Currency = currency,
            UnitAmountMinor = amountMinor,
            Quantity = 1,
            LineTotalAmountMinor = amountMinor
        };
    }

    private static PaymentCheckoutSession CreateCheckoutSession(
        PaymentOrder order,
        Guid nurseProfileId,
        DateTime createdAt,
        string? idempotencyKeyHash = null,
        string? requestFingerprintHash = null)
    {
        return PaymentCheckoutSession.Create(
            order.Id,
            nurseProfileId,
            "TestProvider",
            $"checkout_{Guid.NewGuid():N}",
            order.Currency,
            order.TotalAmountMinor,
            order.ExpiresAt!.Value,
            createdAt.AddMinutes(20),
            idempotencyKeyHash,
            requestFingerprintHash ?? StartMyPaymentCheckoutCommandHandler.ComputeRequestFingerprintHash(nurseProfileId, order.Id));
    }

    private static void SeedNurse(TestPaymentDbContext context, Guid userId, Guid nurseProfileId)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User { Id = userId, Email = "nurse@example.com", IsActive = true };
        user.UserRoles.Add(new UserRole { User = user, UserId = userId, Role = role, RoleId = role.Id });
        context.Users.Add(user);
        context.Roles.Add(role);
        context.NurseProfiles.Add(new NurseProfile { Id = nurseProfileId, UserId = userId });
    }

    private static NursingPlatform.Application.Nurses.Common.NurseRoleGuard CreateGuard(TestPaymentDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NursingPlatform.Application.Nurses.Common.NurseRoleGuard(context, currentUser.Object);
    }

    private static void ForceCheckoutLeaseOwner(PaymentCheckoutSession session, Guid leaseId, DateTime leaseExpiresAt)
    {
        typeof(PaymentCheckoutSession)
            .GetProperty(nameof(PaymentCheckoutSession.ProviderCallLeaseId))!
            .SetValue(session, leaseId);
        typeof(PaymentCheckoutSession)
            .GetProperty(nameof(PaymentCheckoutSession.ProviderCallLeaseExpiresAt))!
            .SetValue(session, leaseExpiresAt);
    }

    private sealed class TestPaymentDbContext : DbContext, IApplicationDbContext
    {
        public TestPaymentDbContext(DbContextOptions<TestPaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<NurseProfile> NurseProfiles => Set<NurseProfile>();
        public DbSet<NurseExperience> NurseExperiences => Set<NurseExperience>();
        public DbSet<NurseEducation> NurseEducation => Set<NurseEducation>();
        public DbSet<NurseCertificate> NurseCertificates => Set<NurseCertificate>();
        public DbSet<NurseLanguage> NurseLanguages => Set<NurseLanguage>();
        public DbSet<NurseSkill> NurseSkills => Set<NurseSkill>();
        public DbSet<NurseCvDocument> NurseCvDocuments => Set<NurseCvDocument>();
        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<EmployerOrganization> EmployerOrganizations => Set<EmployerOrganization>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<ExamCategory> ExamCategories => Set<ExamCategory>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamVersion> ExamVersions => Set<ExamVersion>();
        public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
        public DbSet<ExamAnswerOption> ExamAnswerOptions => Set<ExamAnswerOption>();
        public DbSet<ExamAccessGrant> ExamAccessGrants => Set<ExamAccessGrant>();
        public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
        public DbSet<ExamSessionQuestion> ExamSessionQuestions => Set<ExamSessionQuestion>();
        public DbSet<ExamSessionAnswerOption> ExamSessionAnswerOptions => Set<ExamSessionAnswerOption>();
        public DbSet<ExamSessionAnswer> ExamSessionAnswers => Set<ExamSessionAnswer>();
        public DbSet<PaymentProduct> PaymentProducts => Set<PaymentProduct>();
        public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
        public DbSet<PaymentOrderItem> PaymentOrderItems => Set<PaymentOrderItem>();
        public DbSet<PaymentCheckoutSession> PaymentCheckoutSessions => Set<PaymentCheckoutSession>();
        public bool SimulateCheckoutInsertUniqueRace { get; set; }
        public Func<PaymentCheckoutSession, PaymentCheckoutSession>? WinningCheckoutSessionFactory { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (SimulateCheckoutInsertUniqueRace)
            {
                var attemptedEntry = ChangeTracker.Entries<PaymentCheckoutSession>()
                    .SingleOrDefault(e => e.State == EntityState.Added);

                if (attemptedEntry is not null)
                {
                    SimulateCheckoutInsertUniqueRace = false;
                    var attemptedSession = attemptedEntry.Entity;
                    attemptedEntry.State = EntityState.Detached;
                    var winningSession = WinningCheckoutSessionFactory?.Invoke(attemptedSession)
                        ?? throw new InvalidOperationException("Winning checkout session factory is required.");
                    PaymentCheckoutSessions.Add(winningSession);
                    await base.SaveChangesAsync(cancellationToken);
                    throw new DbUpdateException("Simulated unique checkout insert race.", new InvalidOperationException("Unique constraint violation."));
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public Task<IApplicationDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IApplicationDbTransaction>(new NoopApplicationDbTransaction());
        }

        public Task<int> AcquirePaymentCheckoutProviderLeaseAsync(
            Guid checkoutSessionId,
            Guid leaseId,
            DateTime leaseExpiresAt,
            DateTime timestamp,
            CancellationToken cancellationToken = default)
        {
            var session = PaymentCheckoutSessions.SingleOrDefault(s => s.Id == checkoutSessionId);
            return Task.FromResult(session is not null && session.TryAcquireProviderCallLease(leaseId, leaseExpiresAt, timestamp) ? 1 : 0);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<PaymentOrder>().HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        }

        public Task<int> ExecuteContactRequestTransitionAsync(Guid id, Guid ownerProfileId, bool isEmployerOwner, ContactRequestStatus status, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteExamSessionFinalizationAsync(Guid id, Guid nurseProfileId, ExamSessionStatus status, int score, int maxScore, decimal percentage, bool passed, int correctCount, int questionCount, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class NoopApplicationDbTransaction : IApplicationDbTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeCheckoutProvider : IPaymentCheckoutProvider
    {
        public string ProviderName { get; init; } = "TestProvider";
        public int CallCount { get; private set; }
        public CreatePaymentCheckoutProviderSessionRequest? LastRequest { get; private set; }
        public bool DefinitivelyReject { get; init; }
        public bool ThrowTimeout { get; init; }
        public string CheckoutUrl { get; init; } = "https://checkout.test/session";
        public CancellationTokenSource? CancelCallerDuringProviderCall { get; init; }
        public Action? BeforeThrow { get; init; }

        public Task<CreatePaymentCheckoutProviderSessionResult> CreateCheckoutSessionAsync(
            CreatePaymentCheckoutProviderSessionRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;

            if (ThrowTimeout)
            {
                BeforeThrow?.Invoke();
                throw new TimeoutException("Provider call timed out.");
            }

            if (CancelCallerDuringProviderCall is not null)
            {
                CancelCallerDuringProviderCall.Cancel();
                throw new OperationCanceledException(CancelCallerDuringProviderCall.Token);
            }

            if (DefinitivelyReject)
            {
                BeforeThrow?.Invoke();
                throw new PaymentCheckoutCreationRejectedException();
            }

            return Task.FromResult(new CreatePaymentCheckoutProviderSessionResult
            {
                ProviderCheckoutSessionId = "provider_session",
                ProviderPaymentIntentId = "provider_intent",
                CheckoutUrl = CheckoutUrl,
                ExpiresAt = request.ExpiresAt.AddMinutes(-1)
            });
        }
    }
}
