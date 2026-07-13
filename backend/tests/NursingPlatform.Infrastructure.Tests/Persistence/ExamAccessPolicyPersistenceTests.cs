using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class ExamAccessPolicyPersistenceTests
{
    [Fact]
    public async Task ExamAccessPolicy_WithActivePaidExamAccessProduct_RequiresActiveMatchingGrant()
    {
        await using var context = CreateDbContext();
        var nurseProfileId = Guid.NewGuid();
        var exam = CreateExam(isFree: true);
        context.Exams.Add(exam);
        var examId = exam.Id;
        context.PaymentProducts.Add(CreateProduct(examId));
        await context.SaveChangesAsync();
        var policy = new ExamAccessPolicy(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(nurseProfileId, examId, default));

        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamId = examId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Reason = "Test"
        });
        await context.SaveChangesAsync();

        await policy.AuthorizeStartAsync(nurseProfileId, examId, default);
    }

    [Fact]
    public async Task ExamAccessPolicy_NonFreeExamWithoutProduct_RequiresActiveMatchingGrant()
    {
        await using var context = CreateDbContext();
        var nurseProfileId = Guid.NewGuid();
        var exam = CreateExam(isFree: false);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();
        var policy = new ExamAccessPolicy(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(nurseProfileId, exam.Id, default));

        context.ExamAccessGrants.Add(new ExamAccessGrant
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamId = exam.Id,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = null,
            Reason = "Test"
        });
        await context.SaveChangesAsync();

        await policy.AuthorizeStartAsync(nurseProfileId, exam.Id, default);
    }

    [Fact]
    public async Task ExamAccessPolicy_InactiveZeroPricedAndNonExamAccessProducts_DoNotRequireGrant()
    {
        await using var context = CreateDbContext();
        var inactiveExamId = Guid.NewGuid();
        var zeroPricedExamId = Guid.NewGuid();
        var nonExamAccessExamId = Guid.NewGuid();
        context.Exams.AddRange(
            CreateExam(inactiveExamId, isFree: true),
            CreateExam(zeroPricedExamId, isFree: true),
            CreateExam(nonExamAccessExamId, isFree: true));
        context.PaymentProducts.Add(CreateProduct(inactiveExamId, isActive: false));
        context.PaymentProducts.Add(CreateProduct(zeroPricedExamId, amountMinor: 0));
        context.PaymentProducts.Add(CreateProduct(nonExamAccessExamId, type: (PaymentProductType)999));
        await context.SaveChangesAsync();
        var policy = new ExamAccessPolicy(context);

        await policy.AuthorizeStartAsync(Guid.NewGuid(), inactiveExamId, default);
        await policy.AuthorizeStartAsync(Guid.NewGuid(), zeroPricedExamId, default);
        await policy.AuthorizeStartAsync(Guid.NewGuid(), nonExamAccessExamId, default);
    }

    [Fact]
    public async Task ExamAccessPolicy_GrantMustMatchNurseAndExamAndNotBeExpired()
    {
        var now = DateTime.UtcNow;
        await using var context = CreateDbContext();
        var nurseProfileId = Guid.NewGuid();
        var exam = CreateExam(isFree: true);
        context.Exams.Add(exam);
        var examId = exam.Id;
        context.PaymentProducts.Add(CreateProduct(examId));
        context.ExamAccessGrants.AddRange(
            new ExamAccessGrant
            {
                Id = Guid.NewGuid(),
                NurseProfileId = Guid.NewGuid(),
                ExamId = examId,
                GrantedAt = now,
                ExpiresAt = null,
                Reason = "ForeignNurse"
            },
            new ExamAccessGrant
            {
                Id = Guid.NewGuid(),
                NurseProfileId = nurseProfileId,
                ExamId = Guid.NewGuid(),
                GrantedAt = now,
                ExpiresAt = null,
                Reason = "OtherExam"
            },
            new ExamAccessGrant
            {
                Id = Guid.NewGuid(),
                NurseProfileId = nurseProfileId,
                ExamId = examId,
                GrantedAt = now.AddDays(-2),
                ExpiresAt = now.AddDays(-1),
                Reason = "Expired"
            });
        await context.SaveChangesAsync();
        var policy = new ExamAccessPolicy(context, () => now);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            policy.AuthorizeStartAsync(nurseProfileId, examId, default));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Exam CreateExam(Guid? id = null, bool isFree = true)
    {
        return new Exam
        {
            Id = id ?? Guid.NewGuid(),
            CountryId = Guid.NewGuid(),
            Title = $"Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = isFree
        };
    }

    private static PaymentProduct CreateProduct(
        Guid examId,
        bool isActive = true,
        long amountMinor = 1000,
        PaymentProductType type = PaymentProductType.ExamAccess)
    {
        return new PaymentProduct
        {
            Id = Guid.NewGuid(),
            Type = type,
            ExamId = examId,
            Name = $"Product {Guid.NewGuid():N}",
            Currency = "USD",
            UnitAmountMinor = amountMinor,
            IsActive = isActive
        };
    }
}
