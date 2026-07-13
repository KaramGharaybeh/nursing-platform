using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Commands.CompleteSandboxPaymentCheckout;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public sealed class SandboxPaymentCompletionPostgreSqlTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING";
    private readonly string _databaseName = $"nps_phase8c_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _maintenanceConnectionString;

    public SandboxPaymentCompletionPostgreSqlTests()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL payment completion tests.");
        }

        var testBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _databaseName
        };
        _connectionString = testBuilder.ConnectionString;

        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres"
        };
        _maintenanceConnectionString = maintenanceBuilder.ConnectionString;
    }

    [Fact]
    public async Task CompleteSandboxCheckout_TwoSimultaneousHandlers_ConvergeToOnePaidOrderAndOneGrant()
    {
        var seed = await SeedCompletionGraphAsync(DateTime.UtcNow.AddMinutes(10));

        var firstTask = CompleteAsync(seed.UserId, seed.CheckoutSessionId);
        var secondTask = CompleteAsync(seed.UserId, seed.CheckoutSessionId);

        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.All(results, result => Assert.Equal("Paid", result.OrderStatus));
        Assert.Equal(results[0].PaymentOrderId, results[1].PaymentOrderId);
        Assert.Equal(results[0].PaidAt, results[1].PaidAt);
        Assert.Equal([seed.ExamId], results[0].GrantedExamIds);
        Assert.Equal([seed.ExamId], results[1].GrantedExamIds);

        await using var context = CreateContext();
        var order = await context.PaymentOrders.AsNoTracking().SingleAsync(o => o.Id == seed.OrderId);
        Assert.Equal(PaymentOrderStatus.Paid, order.Status);
        Assert.Equal(order.PaidAt, results[0].PaidAt);
        Assert.Equal(order.PaidAt, results[1].PaidAt);
        var grants = await context.ExamAccessGrants.AsNoTracking()
            .Where(g => g.NurseProfileId == seed.NurseProfileId && g.ExamId == seed.ExamId && g.ExpiresAt == null)
            .ToListAsync();
        Assert.Single(grants);
    }

    [Fact]
    public async Task CompleteSandboxCheckout_WhenGrantInsertViolatesForeignKey_RollsBackPaidTransitionAndGrant()
    {
        var seed = await SeedCompletionGraphAsync(DateTime.UtcNow.AddMinutes(10), examIdSnapshot: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CompleteAsync(seed.UserId, seed.CheckoutSessionId));

        await using var context = CreateContext();
        var order = await context.PaymentOrders.AsNoTracking().SingleAsync(o => o.Id == seed.OrderId);
        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Null(order.PaidAt);
        Assert.Empty(await context.ExamAccessGrants.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CompleteSandboxCheckout_ResponsePaidAtExactlyEqualsPostgreSqlPersistedValue()
    {
        var seed = await SeedCompletionGraphAsync(DateTime.UtcNow.AddMinutes(10));

        var result = await CompleteAsync(seed.UserId, seed.CheckoutSessionId);

        await using var context = CreateContext();
        var persistedPaidAt = await context.PaymentOrders.AsNoTracking()
            .Where(o => o.Id == seed.OrderId)
            .Select(o => o.PaidAt)
            .SingleAsync();
        Assert.Equal(persistedPaidAt, result.PaidAt);
    }

    public async Task InitializeAsync()
    {
        await using (var connection = new NpgsqlConnection(_maintenanceConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE {QuoteIdentifier(_databaseName)}";
            await command.ExecuteNonQueryAsync();
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();

        await using (var terminateCommand = connection.CreateCommand())
        {
            terminateCommand.CommandText = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @databaseName";
            terminateCommand.Parameters.AddWithValue("databaseName", _databaseName);
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using (var dropCommand = connection.CreateCommand())
        {
            dropCommand.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(_databaseName)}";
            await dropCommand.ExecuteNonQueryAsync();
        }
    }

    private async Task<NursingPlatform.Application.Payments.DTOs.PaymentCompletionDto> CompleteAsync(Guid userId, Guid checkoutSessionId)
    {
        await using var context = CreateContext();
        var handler = new CompleteSandboxPaymentCheckoutCommandHandler(context, CreateGuard(context, userId));
        return await handler.Handle(new CompleteSandboxPaymentCheckoutCommand { CheckoutSessionId = checkoutSessionId }, default);
    }

    private async Task<SeedResult> SeedCompletionGraphAsync(DateTime sessionExpiresAt, Guid? examIdSnapshot = null)
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = $"Country {Guid.NewGuid():N}"[..20],
            Code = Guid.NewGuid().ToString("N")[..2].ToUpperInvariant(),
            IsActive = true
        };
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = country.Id,
            Title = $"Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = false
        };
        var product = new PaymentProduct
        {
            Id = Guid.NewGuid(),
            Type = PaymentProductType.ExamAccess,
            ExamId = exam.Id,
            Name = "Exam Access",
            Description = "Exam Access",
            Currency = "USD",
            UnitAmountMinor = 1000,
            IsActive = true
        };
        var item = PaymentOrderItem.CreateSnapshot(product);
        item.ExamIdSnapshot = examIdSnapshot ?? exam.Id;
        var order = PaymentOrder.CreatePending(nurseProfileId, item, DateTime.UtcNow);
        var session = PaymentCheckoutSession.Create(
            order.Id,
            nurseProfileId,
            "Sandbox",
            $"checkout_{Guid.NewGuid():N}",
            order.Currency,
            order.TotalAmountMinor,
            order.ExpiresAt!.Value,
            DateTime.UtcNow.AddMinutes(20),
            null,
            $"fingerprint_{Guid.NewGuid():N}");
        session.MarkProviderPending("sandbox_session", null, "https://sandbox-payments.local/checkout/session", sessionExpiresAt);

        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var user = new User
        {
            Id = userId,
            Email = $"nurse-{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Nurse",
            IsActive = true,
            EmailVerified = true
        };
        user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id, User = user, Role = role });
        context.Countries.Add(country);
        context.Exams.Add(exam);
        context.PaymentProducts.Add(product);
        context.Users.Add(user);
        context.Roles.Add(role);
        context.NurseProfiles.Add(new NurseProfile { Id = nurseProfileId, UserId = userId });
        context.PaymentOrders.Add(order);
        context.PaymentOrderItems.Add(item);
        context.PaymentCheckoutSessions.Add(session);
        await context.SaveChangesAsync();
        return new SeedResult(userId, nurseProfileId, order.Id, session.Id, exam.Id);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static NurseRoleGuard CreateGuard(ApplicationDbContext context, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns(userId);
        currentUser.SetupGet(u => u.IsAuthenticated).Returns(true);
        return new NurseRoleGuard(context, currentUser.Object);
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private sealed record SeedResult(Guid UserId, Guid NurseProfileId, Guid OrderId, Guid CheckoutSessionId, Guid ExamId);
}
