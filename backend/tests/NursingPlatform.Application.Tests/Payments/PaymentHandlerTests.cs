using Microsoft.EntityFrameworkCore;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CancelMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
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

    private static PaymentOrderItem CreateItem()
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Exam Access",
            ProductTypeSnapshot = PaymentProductType.ExamAccess,
            ExamIdSnapshot = Guid.NewGuid(),
            Currency = "USD",
            UnitAmountMinor = 1000,
            Quantity = 1,
            LineTotalAmountMinor = 1000
        };
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
}
