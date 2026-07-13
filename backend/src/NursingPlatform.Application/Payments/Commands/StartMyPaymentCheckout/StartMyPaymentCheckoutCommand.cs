using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Payments;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;

public class StartMyPaymentCheckoutCommand : IRequest<PaymentCheckoutSessionDto>
{
    public Guid OrderId { get; set; }
    public StartPaymentCheckoutRequest Request { get; set; } = new();
}

public class StartMyPaymentCheckoutCommandValidator : AbstractValidator<StartMyPaymentCheckoutCommand>
{
    public StartMyPaymentCheckoutCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.IdempotencyKey)
            .MaximumLength(128)
            .When(x => x.Request is not null && x.Request.IdempotencyKey is not null);
    }
}

public class StartMyPaymentCheckoutCommandHandler : IRequestHandler<StartMyPaymentCheckoutCommand, PaymentCheckoutSessionDto>
{
    private static readonly TimeSpan ProviderCallLeaseDuration = TimeSpan.FromSeconds(30);

    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;
    private readonly IReadOnlyList<IPaymentCheckoutProvider> _providers;

    public StartMyPaymentCheckoutCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        IEnumerable<IPaymentCheckoutProvider> providers)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
        _providers = providers.ToList();
    }

    public async Task<PaymentCheckoutSessionDto> Handle(
        StartMyPaymentCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var idempotencyKeyHash = HashIdempotencyKey(request.Request.IdempotencyKey);
        var requestFingerprintHash = ComputeRequestFingerprintHash(nurseProfileId, request.OrderId);

        PaymentCheckoutSession session;
        Guid leaseId;

        try
        {
            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var order = await _context.PaymentOrders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.NurseProfileId == nurseProfileId, cancellationToken);

            if (order is null)
            {
                throw new KeyNotFoundException("Payment order was not found.");
            }

            if (order.ExpireIfPastDue(now))
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                throw new InvalidOperationException("Payment order has expired.");
            }

            if (order.Status != PaymentOrderStatus.PendingPayment)
            {
                throw new InvalidOperationException("Checkout is only available for pending payment orders.");
            }

            var idempotencySession = idempotencyKeyHash is null
                ? null
                : await _context.PaymentCheckoutSessions
                    .FirstOrDefaultAsync(s => s.NurseProfileId == nurseProfileId
                        && s.IdempotencyKeyHash == idempotencyKeyHash, cancellationToken);

            if (idempotencySession is not null && idempotencySession.PaymentOrderId != order.Id)
            {
                throw new InvalidOperationException("Idempotency key is already bound to a different checkout operation.");
            }

            if (idempotencySession is not null && idempotencySession.RequestFingerprintHash != requestFingerprintHash)
            {
                throw new InvalidOperationException("Idempotency key is already bound to a different checkout request fingerprint.");
            }

            var staleSessions = await _context.PaymentCheckoutSessions
                .Where(s => s.NurseProfileId == nurseProfileId
                    && s.PaymentOrderId == order.Id
                    && (s.Status == PaymentCheckoutSessionStatus.Created
                        || s.Status == PaymentCheckoutSessionStatus.ProviderPending)
                    && s.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            foreach (var staleSession in staleSessions)
            {
                staleSession.ExpireIfPastDue(now);
            }

            if (staleSessions.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (idempotencySession is not null)
            {
                idempotencySession = await _context.PaymentCheckoutSessions
                    .SingleAsync(s => s.Id == idempotencySession.Id, cancellationToken);
                if (idempotencySession.IsTerminal)
                {
                    throw new InvalidOperationException("IdempotencyKeyAlreadyUsed");
                }
            }

            var activeSession = await _context.PaymentCheckoutSessions
                .Where(s => s.NurseProfileId == nurseProfileId
                    && s.PaymentOrderId == order.Id
                    && (s.Status == PaymentCheckoutSessionStatus.Created
                        || s.Status == PaymentCheckoutSessionStatus.ProviderPending)
                    && s.ExpiresAt > now)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeSession is not null && activeSession.Status == PaymentCheckoutSessionStatus.ProviderPending)
            {
                await transaction.CommitAsync(cancellationToken);
                return PaymentMapping.ToCheckoutSessionDto(activeSession);
            }

            leaseId = Guid.NewGuid();
            var leaseExpiresAt = now.Add(ProviderCallLeaseDuration);

            if (activeSession is not null)
            {
                if (activeSession.HasActiveProviderCallLease(now))
                {
                    await transaction.CommitAsync(cancellationToken);
                    throw CreateCheckoutInitializationInProgress(activeSession.ProviderCallLeaseExpiresAt!.Value, now);
                }

                ResolveProviderForCall();

                var acquired = await _context.AcquirePaymentCheckoutProviderLeaseAsync(
                    activeSession.Id,
                    leaseId,
                    leaseExpiresAt,
                    now,
                    cancellationToken);

                if (acquired == 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    throw CreateCheckoutInitializationInProgress(activeSession.ProviderCallLeaseExpiresAt ?? leaseExpiresAt, now);
                }

                session = await _context.PaymentCheckoutSessions
                    .AsNoTracking()
                    .SingleAsync(s => s.Id == activeSession.Id, cancellationToken);
            }
            else
            {
                var provider = ResolveProviderForCall();
                session = PaymentCheckoutSession.Create(
                    order.Id,
                    nurseProfileId,
                    provider.ProviderName,
                    $"checkout_{Guid.NewGuid():N}",
                    order.Currency,
                    order.TotalAmountMinor,
                    order.ExpiresAt!.Value,
                    order.ExpiresAt.Value,
                    idempotencyKeyHash,
                    requestFingerprintHash);

                if (!session.TryAcquireProviderCallLease(leaseId, leaseExpiresAt, now))
                {
                    throw CreateCheckoutInitializationInProgress(leaseExpiresAt, now);
                }

                _context.PaymentCheckoutSessions.Add(session);
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    DetachEntity(session);
                    throw new CheckoutSessionInsertRaceException(exception);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (CheckoutSessionInsertRaceException)
        {
            return await RecoverFromCheckoutSessionInsertRaceAsync(nurseProfileId, request.OrderId, cancellationToken);
        }

        try
        {
            var providerResult = await CreateProviderSessionSafelyAsync(session, cancellationToken);
            return await PersistProviderOutcomeAsync(session.Id, leaseId, providerResult, cancellationToken);
        }
        catch (PaymentCheckoutCreationRejectedException)
        {
            await PersistDefinitiveProviderRejectionAsync(session.Id, leaseId, cancellationToken);
            throw new InvalidOperationException("Payment checkout creation was rejected by the provider.");
        }
    }

    public static string? HashIdempotencyKey(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        return "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(idempotencyKey.Trim()))).ToLowerInvariant();
    }

    public static string ComputeRequestFingerprintHash(Guid nurseProfileId, Guid paymentOrderId)
    {
        var canonical = string.Join('\n',
            "v1",
            "operation=StartPaymentCheckout",
            $"nurseProfileId={nurseProfileId:D}",
            $"paymentOrderId={paymentOrderId:D}",
            "businessFields={}");

        return "v1:sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private async Task<CreatePaymentCheckoutProviderSessionResult> CreateProviderSessionSafelyAsync(
        PaymentCheckoutSession session,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ResolveProviderForCall().CreateCheckoutSessionAsync(new CreatePaymentCheckoutProviderSessionRequest
            {
                PaymentOrderId = session.PaymentOrderId,
                CheckoutSessionId = session.Id,
                ProviderClientReference = session.ProviderClientReference,
                Currency = session.Currency,
                AmountMinor = session.AmountMinor,
                Description = "Payment order checkout",
                SuccessUrl = "https://nursing-platform.local/payments/checkout/success",
                CancelUrl = "https://nursing-platform.local/payments/checkout/cancel",
                ExpiresAt = session.ExpiresAt
            }, cancellationToken);
        }
        catch (Exception exception) when (IsRecoverableProviderOutcome(exception, cancellationToken))
        {
            throw new InvalidOperationException("Payment provider outcome is unknown. Checkout remains recoverable.", exception);
        }
    }

    private async Task<PaymentCheckoutSessionDto> PersistProviderOutcomeAsync(
        Guid sessionId,
        Guid leaseId,
        CreatePaymentCheckoutProviderSessionResult providerResult,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var session = await _context.PaymentCheckoutSessions
            .SingleAsync(s => s.Id == sessionId, cancellationToken);

        if (session.Status == PaymentCheckoutSessionStatus.ProviderPending)
        {
            await transaction.CommitAsync(cancellationToken);
            return PaymentMapping.ToCheckoutSessionDto(session);
        }

        if (session.Status != PaymentCheckoutSessionStatus.Created || !session.IsProviderCallLeaseOwner(leaseId))
        {
            throw new InvalidOperationException("Checkout initialization is not owned by this request.");
        }

        if (session.ExpireIfPastDue(now))
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new InvalidOperationException("Checkout session has expired.");
        }

        session.MarkProviderPending(
            providerResult.ProviderCheckoutSessionId,
            providerResult.ProviderPaymentIntentId,
            providerResult.CheckoutUrl,
            providerResult.ExpiresAt);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return PaymentMapping.ToCheckoutSessionDto(session);
    }

    private async Task PersistDefinitiveProviderRejectionAsync(
        Guid sessionId,
        Guid leaseId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var session = await _context.PaymentCheckoutSessions
            .SingleAsync(s => s.Id == sessionId, cancellationToken);

        if (session.Status == PaymentCheckoutSessionStatus.Created && session.IsProviderCallLeaseOwner(leaseId))
        {
            session.MarkCreationRejected();
            await _context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static CheckoutInitializationInProgressException CreateCheckoutInitializationInProgress(DateTime leaseExpiresAt, DateTime timestamp)
    {
        var retryAfter = leaseExpiresAt > timestamp
            ? leaseExpiresAt - timestamp
            : TimeSpan.Zero;

        if (retryAfter > ProviderCallLeaseDuration)
        {
            retryAfter = ProviderCallLeaseDuration;
        }

        return new CheckoutInitializationInProgressException(retryAfter);
    }

    private IPaymentCheckoutProvider ResolveProviderForCall()
    {
        return _providers.Count switch
        {
            0 => throw new InvalidOperationException("Payment checkout provider is unavailable."),
            1 => _providers[0],
            _ => throw new InvalidOperationException("Multiple payment checkout providers are configured.")
        };
    }

    private async Task<PaymentCheckoutSessionDto> RecoverFromCheckoutSessionInsertRaceAsync(
        Guid nurseProfileId,
        Guid paymentOrderId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var winningSession = await _context.PaymentCheckoutSessions
            .Where(s => s.NurseProfileId == nurseProfileId
                && s.PaymentOrderId == paymentOrderId
                && (s.Status == PaymentCheckoutSessionStatus.Created
                    || s.Status == PaymentCheckoutSessionStatus.ProviderPending)
                && s.ExpiresAt > now)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (winningSession is null)
        {
            throw CreateCheckoutInitializationInProgress(now.Add(ProviderCallLeaseDuration), now);
        }

        if (winningSession.Status == PaymentCheckoutSessionStatus.ProviderPending)
        {
            return PaymentMapping.ToCheckoutSessionDto(winningSession);
        }

        throw CreateCheckoutInitializationInProgress(
            winningSession.ProviderCallLeaseExpiresAt ?? now.Add(ProviderCallLeaseDuration),
            now);
    }

    private void DetachEntity(object entity)
    {
        if (_context is DbContext dbContext)
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }
    }

    private static bool IsRecoverableProviderOutcome(Exception exception, CancellationToken cancellationToken)
    {
        return exception is TimeoutException
            or HttpRequestException
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);
    }

    private sealed class CheckoutSessionInsertRaceException : Exception
    {
        public CheckoutSessionInsertRaceException(Exception innerException)
            : base("A concurrent checkout session insert won the race.", innerException)
        {
        }
    }
}
