using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Commands.CompleteSandboxPaymentCheckout;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Payments.Sandbox;
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

    [Fact]
    public async Task ExamAccessPolicy_WithPostgreSql_IdentifiesPaidProductsAndActiveMatchingGrants()
    {
        var seed = await SeedCompletionGraphAsync(DateTime.UtcNow.AddMinutes(10));

        await using var context = CreateContext();
        var policy = new ExamAccessPolicy(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(seed.NurseProfileId, seed.ExamId, default));

        var countryId = await context.Exams
            .Where(e => e.Id == seed.ExamId)
            .Select(e => e.CountryId)
            .SingleAsync();
        var foreignUserId = Guid.NewGuid();
        var foreignNurseProfileId = Guid.NewGuid();
        var otherExamId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = foreignUserId,
            Email = $"foreign-nurse-{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            FirstName = "Foreign",
            LastName = "Nurse",
            IsActive = true,
            EmailVerified = true
        });
        context.NurseProfiles.Add(new NurseProfile { Id = foreignNurseProfileId, UserId = foreignUserId });
        context.Exams.Add(new Exam
        {
            Id = otherExamId,
            CountryId = countryId,
            Title = $"Other Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = false
        });

        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = foreignNurseProfileId,
            ExamId = seed.ExamId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = null,
            Reason = "ForeignNurse"
        });
        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = seed.NurseProfileId,
            ExamId = otherExamId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = null,
            Reason = "OtherExam"
        });
        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = seed.NurseProfileId,
            ExamId = seed.ExamId,
            GrantedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Reason = "Expired"
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(seed.NurseProfileId, seed.ExamId, default));

        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = seed.NurseProfileId,
            ExamId = seed.ExamId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Reason = "Active"
        });
        await context.SaveChangesAsync();

        await policy.AuthorizeStartAsync(seed.NurseProfileId, seed.ExamId, default);
    }

    [Fact]
    public async Task SandboxPurchaseToExamStart_BackendFlow_GrantsAccessOnlyToPurchasingNurseAndDoesNotConsumeGrant()
    {
        var seed = await SeedPurchaseStartGraphAsync();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => StartExamAsync(seed.NurseUserId, seed.ExamId));

        var order = await CreateOrderAsync(seed.NurseUserId, seed.ProductId);
        var checkout = await StartCheckoutAsync(seed.NurseUserId, order.Id);
        var completion = await CompleteAsync(seed.NurseUserId, checkout.Id);

        await using (var context = CreateContext())
        {
            var paidOrder = await context.PaymentOrders.AsNoTracking().SingleAsync(o => o.Id == order.Id);
            Assert.Equal(PaymentOrderStatus.Paid, paidOrder.Status);
            Assert.Equal("Paid", completion.OrderStatus);

            var grants = await context.ExamAccessGrants.AsNoTracking()
                .Where(g => g.NurseProfileId == seed.NurseProfileId && g.ExamId == seed.ExamId && g.ExpiresAt == null)
                .ToListAsync();
            Assert.Single(grants);
        }

        ExamAccessGrant grantBeforeStart;
        await using (var context = CreateContext())
        {
            grantBeforeStart = await context.ExamAccessGrants.AsNoTracking()
                .SingleAsync(g => g.NurseProfileId == seed.NurseProfileId && g.ExamId == seed.ExamId && g.ExpiresAt == null);
        }

        var started = await StartExamAsync(seed.NurseUserId, seed.ExamId);

        Assert.Equal(seed.ExamId, started.ExamId);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => StartExamAsync(seed.OtherNurseUserId, seed.ExamId));

        await using (var context = CreateContext())
        {
            var grantAfterStart = await context.ExamAccessGrants.AsNoTracking()
                .SingleAsync(g => g.NurseProfileId == seed.NurseProfileId && g.ExamId == seed.ExamId && g.ExpiresAt == null);
            Assert.Equal(grantBeforeStart.Id, grantAfterStart.Id);
            Assert.Equal(grantBeforeStart.GrantedAt, grantAfterStart.GrantedAt);
            Assert.Equal(grantBeforeStart.ExpiresAt, grantAfterStart.ExpiresAt);
            Assert.Equal(grantBeforeStart.Reason, grantAfterStart.Reason);
        }

        var repeatedCompletion = await CompleteAsync(seed.NurseUserId, checkout.Id);
        Assert.Equal("Paid", repeatedCompletion.OrderStatus);

        await using (var context = CreateContext())
        {
            var grants = await context.ExamAccessGrants.AsNoTracking()
                .Where(g => g.NurseProfileId == seed.NurseProfileId && g.ExamId == seed.ExamId && g.ExpiresAt == null)
                .ToListAsync();
            var grant = Assert.Single(grants);
            Assert.Null(grant.ExpiresAt);
            Assert.Equal("SandboxPaymentCompletion", grant.Reason);
        }
    }

    [Fact]
    public async Task StartExamSession_WithPostgreSql_NonFreeExamWithoutProductRequiresGrant()
    {
        var seed = await SeedNonFreeExamWithoutProductStartGraphAsync();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => StartExamAsync(seed.NurseUserId, seed.ExamId));

        await using (var context = CreateContext())
        {
            Assert.Empty(await context.ExamSessions.AsNoTracking()
                .Where(s => s.NurseProfileId == seed.NurseProfileId && s.ExamId == seed.ExamId)
                .ToListAsync());

            context.ExamAccessGrants.Add(new ExamAccessGrant
            {
                Id = Guid.NewGuid(),
                NurseProfileId = seed.NurseProfileId,
                ExamId = seed.ExamId,
                GrantedAt = DateTime.UtcNow,
                ExpiresAt = null,
                Reason = "Test"
            });
            await context.SaveChangesAsync();
        }

        var started = await StartExamAsync(seed.NurseUserId, seed.ExamId);

        Assert.Equal(seed.ExamId, started.ExamId);
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

    private async Task<NursingPlatform.Application.Payments.DTOs.PaymentOrderDto> CreateOrderAsync(Guid userId, Guid productId)
    {
        await using var context = CreateContext();
        var handler = new CreateMyPaymentOrderCommandHandler(context, CreateGuard(context, userId));
        return await handler.Handle(new CreateMyPaymentOrderCommand
        {
            Request = new CreatePaymentOrderRequest { ProductId = productId }
        }, default);
    }

    private async Task<NursingPlatform.Application.Payments.DTOs.PaymentCheckoutSessionDto> StartCheckoutAsync(Guid userId, Guid orderId)
    {
        await using var context = CreateContext();
        var provider = new SandboxPaymentCheckoutProvider(Options.Create(new SandboxPaymentSettings
        {
            PublicBaseUrl = "https://sandbox-payments.local",
            SupportedCurrency = "USD"
        }));
        var handler = new StartMyPaymentCheckoutCommandHandler(context, CreateGuard(context, userId), [provider]);
        return await handler.Handle(new StartMyPaymentCheckoutCommand { OrderId = orderId }, default);
    }

    private async Task<NursingPlatform.Application.Exams.DTOs.ExamSessionDto> StartExamAsync(Guid userId, Guid examId)
    {
        await using var context = CreateContext();
        var handler = new StartExamSessionCommandHandler(context, CreateGuard(context, userId), new ExamAccessPolicy(context));
        return await handler.Handle(new StartExamSessionCommand { ExamId = examId }, default);
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

    private async Task<PurchaseStartSeedResult> SeedPurchaseStartGraphAsync()
    {
        await using var context = CreateContext();
        var nurseUserId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        var otherNurseUserId = Guid.NewGuid();
        var otherNurseProfileId = Guid.NewGuid();
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
        var version = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            VersionNumber = 1,
            Status = ExamVersionStatus.Published
        };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
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
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var nurseUser = CreateNurseUser(nurseUserId, role);
        var otherNurseUser = CreateNurseUser(otherNurseUserId, role);

        context.Countries.Add(country);
        context.Exams.Add(exam);
        context.ExamVersions.Add(version);
        context.ExamQuestions.Add(question);
        context.ExamAnswerOptions.AddRange(
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 1,
                OptionText = "A",
                IsCorrect = true,
                IsActive = true
            },
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 2,
                OptionText = "B",
                IsCorrect = false,
                IsActive = true
            });
        context.PaymentProducts.Add(product);
        context.Roles.Add(role);
        context.Users.AddRange(nurseUser, otherNurseUser);
        context.NurseProfiles.AddRange(
            new NurseProfile { Id = nurseProfileId, UserId = nurseUserId },
            new NurseProfile { Id = otherNurseProfileId, UserId = otherNurseUserId });
        await context.SaveChangesAsync();

        return new PurchaseStartSeedResult(nurseUserId, nurseProfileId, otherNurseUserId, product.Id, exam.Id);
    }

    private async Task<NonFreeExamWithoutProductStartSeedResult> SeedNonFreeExamWithoutProductStartGraphAsync()
    {
        await using var context = CreateContext();
        var nurseUserId = Guid.NewGuid();
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
        var version = new ExamVersion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            VersionNumber = 1,
            Status = ExamVersionStatus.Published
        };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        var role = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var nurseUser = CreateNurseUser(nurseUserId, role);

        context.Countries.Add(country);
        context.Exams.Add(exam);
        context.ExamVersions.Add(version);
        context.ExamQuestions.Add(question);
        context.ExamAnswerOptions.AddRange(
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 1,
                OptionText = "A",
                IsCorrect = true,
                IsActive = true
            },
            new ExamAnswerOption
            {
                Id = Guid.NewGuid(),
                ExamQuestionId = question.Id,
                DisplayOrder = 2,
                OptionText = "B",
                IsCorrect = false,
                IsActive = true
            });
        context.Roles.Add(role);
        context.Users.Add(nurseUser);
        context.NurseProfiles.Add(new NurseProfile { Id = nurseProfileId, UserId = nurseUserId });
        await context.SaveChangesAsync();

        return new NonFreeExamWithoutProductStartSeedResult(nurseUserId, nurseProfileId, exam.Id);
    }

    private static User CreateNurseUser(Guid userId, Role role)
    {
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
        return user;
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
    private sealed record PurchaseStartSeedResult(Guid NurseUserId, Guid NurseProfileId, Guid OtherNurseUserId, Guid ProductId, Guid ExamId);
    private sealed record NonFreeExamWithoutProductStartSeedResult(Guid NurseUserId, Guid NurseProfileId, Guid ExamId);
}
