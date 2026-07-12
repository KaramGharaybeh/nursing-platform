using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Exams.Analytics;

public class ExamAnalyticsHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_Summary_WhenNurseProfileMissing_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        SetupContext(userId, [], [], [], [], []);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetMyExamAnalyticsSummaryQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Summary_IncludesOnlyCurrentNurseSessions()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80, startedAt: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(Guid.NewGuid(), fixture.Exams[0].Id, ExamSessionStatus.Submitted, 10, startedAt: new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc))
        ]);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new GetMyExamAnalyticsSummaryQuery(), CancellationToken.None);

        Assert.Equal(1, result.AttemptCount);
        Assert.Equal(80, result.AverageScorePercentage);
    }

    [Fact]
    public async Task Handle_Summary_CountsSubmittedExpiredAbandonedAndInProgressStatuses()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Expired, 60),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Abandoned, 0),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.InProgress, 0)
        ]);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new GetMyExamAnalyticsSummaryQuery(), CancellationToken.None);

        Assert.Equal(1, result.SubmittedCount);
        Assert.Equal(1, result.ExpiredCount);
        Assert.Equal(1, result.AbandonedCount);
        Assert.Equal(1, result.InProgressCount);
        Assert.Equal(4, result.AttemptCount);
        Assert.Equal(2, result.CountedAttemptCount);
    }

    [Fact]
    public async Task Handle_Summary_ExcludesInProgressAndAbandonedFromScoreAndPassMetrics()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 90, passed: true),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Expired, 50, passed: false),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Abandoned, 100, passed: true),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.InProgress, 100, passed: true)
        ]);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new GetMyExamAnalyticsSummaryQuery(), CancellationToken.None);

        Assert.Equal(2, result.CountedAttemptCount);
        Assert.Equal(1, result.PassedCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(50, result.PassRatePercentage);
        Assert.Equal(70, result.AverageScorePercentage);
        Assert.Equal(90, result.BestScorePercentage);
    }

    [Fact]
    public async Task Handle_Summary_WithNoCountedAttempts_ReturnsZeroCountsAndNullRates()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.InProgress, 0),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Abandoned, 0)
        ]);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new GetMyExamAnalyticsSummaryQuery(), CancellationToken.None);

        Assert.Equal(2, result.AttemptCount);
        Assert.Equal(0, result.CountedAttemptCount);
        Assert.Null(result.PassRatePercentage);
        Assert.Null(result.AverageScorePercentage);
        Assert.Null(result.BestScorePercentage);
        Assert.Null(result.LatestScorePercentage);
    }

    [Fact]
    public async Task Handle_Summary_AppliesCountryCategoryAndExamFiltersAfterOwnership()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[1].Id, ExamSessionStatus.Submitted, 60)
        ]);
        var handler = new GetMyExamAnalyticsSummaryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new GetMyExamAnalyticsSummaryQuery
        {
            CountryId = fixture.Countries[0].Id,
            CategoryId = fixture.Categories[0].Id,
            ExamId = fixture.Exams[0].Id
        }, CancellationToken.None);

        Assert.Equal(1, result.AttemptCount);
        Assert.Equal(80, result.AverageScorePercentage);
    }

    [Fact]
    public async Task Handle_ByExam_PaginatesAndProvesSkipAndTake()
    {
        var fixture = CreateFixture(examCount: 3);
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
            fixture.Exams.Select((exam, index) => CreateSession(fixture.NurseProfile.Id, exam.Id, ExamSessionStatus.Submitted, 80 - index)).ToList());
        var handler = new ListMyExamAnalyticsByExamQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new ListMyExamAnalyticsByExamQuery { Page = 2, PageSize = 1 }, CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Exam 2", result.Items[0].ExamTitle);
    }

    [Fact]
    public async Task Handle_ByCategory_GroupsNullCategorySafely()
    {
        var fixture = CreateFixture(includeCategory: false);
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80)
        ]);
        var handler = new ListMyExamAnalyticsByCategoryQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new ListMyExamAnalyticsByCategoryQuery(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].CategoryId);
        Assert.Null(result.Items[0].CategoryName);
        Assert.Equal(fixture.Countries[0].Id, result.Items[0].CountryId);
    }

    [Fact]
    public async Task Handle_Trends_BucketWithAllStatusesCountsAttemptAndCountedAttemptSeparately()
    {
        var fixture = CreateFixture();
        var startedAt = new DateTime(2026, 7, 12, 10, 30, 0, DateTimeKind.Utc);
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80, startedAt: startedAt),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Expired, 60, startedAt: startedAt.AddHours(1)),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Abandoned, 100, startedAt: startedAt.AddHours(2), passed: true),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.InProgress, 100, startedAt: startedAt.AddHours(3), passed: true)
        ]);
        var handler = new ListMyExamAnalyticsTrendsQueryHandler(_contextMock.Object, CreateGuard());

        var result = await handler.Handle(new ListMyExamAnalyticsTrendsQuery { Bucket = Application.Exams.Analytics.Common.ExamAnalyticsBucket.Day }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(4, result[0].AttemptCount);
        Assert.Equal(2, result[0].CountedAttemptCount);
        Assert.Equal(70, result[0].AverageScorePercentage);
        Assert.Equal(50, result[0].PassRatePercentage);
    }

    [Fact]
    public async Task Handle_Trends_GroupsByWeekAndMonthDeterministically()
    {
        var fixture = CreateFixture();
        SetupContext(fixture.UserId, [fixture.NurseProfile], fixture.Countries, fixture.Categories, fixture.Exams,
        [
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 80, startedAt: new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc)),
            CreateSession(fixture.NurseProfile.Id, fixture.Exams[0].Id, ExamSessionStatus.Submitted, 90, startedAt: new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc))
        ]);
        var handler = new ListMyExamAnalyticsTrendsQueryHandler(_contextMock.Object, CreateGuard());

        var weeks = await handler.Handle(new ListMyExamAnalyticsTrendsQuery { Bucket = Application.Exams.Analytics.Common.ExamAnalyticsBucket.Week }, CancellationToken.None);
        var months = await handler.Handle(new ListMyExamAnalyticsTrendsQuery { Bucket = Application.Exams.Analytics.Common.ExamAnalyticsBucket.Month }, CancellationToken.None);

        Assert.Equal(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc), weeks[0].BucketStart);
        Assert.Equal(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc), weeks[0].BucketEnd);
        Assert.Equal(new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc), weeks[1].BucketStart);
        Assert.Equal(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), months[0].BucketStart);
        Assert.Equal(new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), months[0].BucketEnd);
        Assert.Equal(new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), months[1].BucketStart);
    }

    private NurseRoleGuard CreateGuard() => new(_contextMock.Object, _currentUserMock.Object);

    private void SetupContext(
        Guid? currentUserId,
        IReadOnlyCollection<NurseProfile> nurseProfiles,
        IReadOnlyCollection<Country> countries,
        IReadOnlyCollection<ExamCategory> categories,
        IReadOnlyCollection<Exam> exams,
        IReadOnlyCollection<ExamSession> sessions)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { CreateNurseUser(currentUserId ?? Guid.NewGuid()) }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(nurseProfiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ExamCategories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Exams).Returns(exams.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ExamSessions).Returns(sessions.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ExamSessionAnswers).Throws(new InvalidOperationException("ExamSessionAnswers must not be read."));
        _contextMock.Setup(c => c.ExamSessionAnswerOptions).Throws(new InvalidOperationException("ExamSessionAnswerOptions must not be read."));
    }

    private static ExamSession CreateSession(
        Guid nurseProfileId,
        Guid examId,
        ExamSessionStatus status,
        decimal percentage,
        DateTime? startedAt = null,
        bool? passed = null)
    {
        return new ExamSession
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamId = examId,
            ExamVersionId = Guid.NewGuid(),
            Status = status,
            StartedAt = startedAt ?? new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpiresAt = (startedAt ?? new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)).AddHours(1),
            Score = (int)percentage,
            MaxScore = 100,
            Percentage = percentage,
            Passed = passed ?? percentage >= 70,
            CorrectCount = (int)percentage,
            QuestionCount = 100,
            FinalizedAt = status is ExamSessionStatus.Submitted or ExamSessionStatus.Expired ? startedAt : null
        };
    }

    private static AnalyticsFixture CreateFixture(int examCount = 2, bool includeCategory = true)
    {
        var userId = Guid.NewGuid();
        var nurseProfile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var country = new Country { Id = Guid.NewGuid(), Name = "United States", Code = "US", IsActive = true };
        var category = new ExamCategory { Id = Guid.NewGuid(), CountryId = country.Id, Name = "NCLEX", Slug = "nclex", IsActive = true };
        var exams = Enumerable.Range(1, examCount)
            .Select(i => new Exam
            {
                Id = Guid.NewGuid(),
                CountryId = country.Id,
                ExamCategoryId = includeCategory ? category.Id : null,
                Title = $"Exam {i}",
                Slug = $"exam-{i}",
                DurationMinutes = 60,
                PassingScorePercentage = 70,
                Status = ExamStatus.Published,
                IsFree = true
            })
            .ToList();

        return new AnalyticsFixture(
            userId,
            nurseProfile,
            [country],
            includeCategory ? [category] : [],
            exams);
    }

    private static User CreateNurseUser(Guid userId)
    {
        var roleId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            IsActive = true,
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role { Id = roleId, Name = "Nurse" }
                }
            ]
        };
    }

    private sealed record AnalyticsFixture(
        Guid UserId,
        NurseProfile NurseProfile,
        List<Country> Countries,
        List<ExamCategory> Categories,
        List<Exam> Exams);
}
