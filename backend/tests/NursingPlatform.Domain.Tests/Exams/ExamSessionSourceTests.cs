using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Domain.Tests.Exams;

public class ExamSessionSourceTests
{
    [Fact]
    public void ExamSession_CreateFreeSource_RecordsImmutableFreeSource()
    {
        var session = ExamSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc),
            60,
            ExamSessionSource.Free);

        Assert.Equal(ExamSessionSource.Free, session.Source);
        AssertSourceHasNoPublicSetter();
    }

    [Fact]
    public void ExamSession_CreateStandaloneGrantSource_RecordsImmutableStandaloneGrantSource()
    {
        var session = ExamSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc),
            60,
            ExamSessionSource.StandaloneGrant);

        Assert.Equal(ExamSessionSource.StandaloneGrant, session.Source);
        AssertSourceHasNoPublicSetter();
    }

    [Fact]
    public void ExamSession_CreatePackageAttemptSource_RecordsImmutablePackageSource()
    {
        var session = ExamSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc),
            60,
            ExamSessionSource.PackageAttempt);

        Assert.Equal(ExamSessionSource.PackageAttempt, session.Source);
        AssertSourceHasNoPublicSetter();
    }

    private static void AssertSourceHasNoPublicSetter()
    {
        var property = typeof(ExamSession).GetProperty(nameof(ExamSession.Source));

        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic == true);
    }
}
