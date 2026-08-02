using System.Reflection;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Domain.Tests.PreparationPackages;

public class PackagePracticeProgressDomainTests
{
    [Fact]
    public void Create_WhenAnswerIsCorrect_CapturesScopeAnswerStateAndTimestamp()
    {
        var facts = CreateProgressFacts(isCorrect: true);

        var progress = CreateProgress(facts);

        Assert.NotEqual(Guid.Empty, progress.Id);
        Assert.Equal(facts.NurseProfileId, progress.NurseProfileId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, progress.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PracticeCollectionVersionId, progress.PracticeCollectionVersionId);
        Assert.Equal(facts.PracticeItemId, progress.PracticeItemId);
        Assert.Equal(facts.SelectedPracticeAnswerOptionId, progress.SelectedPracticeAnswerOptionId);
        Assert.Equal(PackagePracticeProgressState.AnsweredCorrect, progress.State);
        Assert.True(progress.IsCorrect);
        Assert.Equal(facts.AnsweredAt, progress.LastAnsweredAt);
        Assert.Equal(facts.AnsweredAt, progress.CreatedAt);
        Assert.Equal(facts.AnsweredAt, progress.UpdatedAt);
    }

    [Fact]
    public void Create_WhenAnswerIsIncorrect_CapturesScopeAnswerStateAndTimestamp()
    {
        var facts = CreateProgressFacts(isCorrect: false);

        var progress = CreateProgress(facts);

        Assert.Equal(facts.NurseProfileId, progress.NurseProfileId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, progress.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PracticeCollectionVersionId, progress.PracticeCollectionVersionId);
        Assert.Equal(facts.PracticeItemId, progress.PracticeItemId);
        Assert.Equal(facts.SelectedPracticeAnswerOptionId, progress.SelectedPracticeAnswerOptionId);
        Assert.Equal(PackagePracticeProgressState.AnsweredIncorrect, progress.State);
        Assert.False(progress.IsCorrect);
        Assert.Equal(facts.AnsweredAt, progress.LastAnsweredAt);
    }

    [Fact]
    public void UpdateAnswer_FromIncorrectToCorrect_OverwritesLatestAnswerStateAndTimestampOnly()
    {
        var facts = CreateProgressFacts(isCorrect: false);
        var progress = CreateProgress(facts);
        var newSelectedAnswerOptionId = Guid.NewGuid();
        var newAnsweredAt = facts.AnsweredAt.AddMinutes(5);

        progress.UpdateAnswer(newSelectedAnswerOptionId, isCorrect: true, newAnsweredAt);

        Assert.Equal(facts.NurseProfileId, progress.NurseProfileId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, progress.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PracticeCollectionVersionId, progress.PracticeCollectionVersionId);
        Assert.Equal(facts.PracticeItemId, progress.PracticeItemId);
        Assert.Equal(newSelectedAnswerOptionId, progress.SelectedPracticeAnswerOptionId);
        Assert.Equal(PackagePracticeProgressState.AnsweredCorrect, progress.State);
        Assert.True(progress.IsCorrect);
        Assert.Equal(newAnsweredAt, progress.LastAnsweredAt);
        Assert.Equal(facts.AnsweredAt, progress.CreatedAt);
        Assert.Equal(newAnsweredAt, progress.UpdatedAt);
    }

    [Fact]
    public void UpdateAnswer_FromCorrectToIncorrect_OverwritesLatestAnswerStateAndTimestampOnly()
    {
        var facts = CreateProgressFacts(isCorrect: true);
        var progress = CreateProgress(facts);
        var newSelectedAnswerOptionId = Guid.NewGuid();
        var newAnsweredAt = facts.AnsweredAt.AddMinutes(5);

        progress.UpdateAnswer(newSelectedAnswerOptionId, isCorrect: false, newAnsweredAt);

        Assert.Equal(facts.NurseProfileId, progress.NurseProfileId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, progress.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PracticeCollectionVersionId, progress.PracticeCollectionVersionId);
        Assert.Equal(facts.PracticeItemId, progress.PracticeItemId);
        Assert.Equal(newSelectedAnswerOptionId, progress.SelectedPracticeAnswerOptionId);
        Assert.Equal(PackagePracticeProgressState.AnsweredIncorrect, progress.State);
        Assert.False(progress.IsCorrect);
        Assert.Equal(newAnsweredAt, progress.LastAnsweredAt);
        Assert.Equal(facts.AnsweredAt, progress.CreatedAt);
        Assert.Equal(newAnsweredAt, progress.UpdatedAt);
    }

    [Fact]
    public void Create_RejectsEmptyRequiredIds()
    {
        var facts = CreateProgressFacts(isCorrect: true);

        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with { NurseProfileId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with { PackagePurchaseEntitlementId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with { PracticeCollectionVersionId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with { PracticeItemId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with { SelectedPracticeAnswerOptionId = Guid.Empty }));
    }

    [Fact]
    public void UpdateAnswer_RejectsEmptySelectedPracticeAnswerOptionId()
    {
        var progress = CreateProgress();

        Assert.Throws<InvalidOperationException>(() => progress.UpdateAnswer(
            Guid.Empty,
            isCorrect: true,
            new DateTime(2026, 8, 2, 12, 5, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void CreateAndUpdateAnswer_RequireUtcTimestamps()
    {
        var facts = CreateProgressFacts(isCorrect: true);
        var progress = CreateProgress(facts);

        Assert.Throws<InvalidOperationException>(() => CreateProgress(facts with
        {
            AnsweredAt = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Local)
        }));
        Assert.Throws<InvalidOperationException>(() => progress.UpdateAnswer(
            Guid.NewGuid(),
            isCorrect: false,
            new DateTime(2026, 8, 2, 12, 5, 0, DateTimeKind.Local)));
    }

    [Fact]
    public void PackagePracticeProgress_DoesNotExposeOfficialExamIdentifiersOrSnapshots()
    {
        var forbiddenTerms = new[]
        {
            "Exam" + "Question",
            "Exam" + "Question" + "Id",
            "Exam" + "Answer" + "Option",
            "Exam" + "Answer" + "Option" + "Id",
            "Exam" + "Session",
            "Exam" + "Session" + "Id",
            "Question" + "Text" + "Snapshot",
            "Option" + "Text" + "Snapshot",
            "Correct" + "Answer",
            "Correct" + "Option",
            "Answer" + "Key",
            "Ration" + "ale",
            "Explanation" + "Snapshot"
        };

        var exposedNames = new[] { typeof(PackagePracticeProgress) }
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(property => property.Name)
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(field => field.Name))
                .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(method => method.Name))
                .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .SelectMany(method => method.GetParameters().Select(parameter => parameter.Name ?? string.Empty))))
            .ToList();

        Assert.All(exposedNames, name =>
        {
            Assert.DoesNotContain(forbiddenTerms, term =>
                name.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void PublicDomainSurface_DoesNotExposeExamAttemptConsumptionBehavior()
    {
        var forbiddenMethodFragments = new[]
        {
            "Consume",
            "Attempt",
            "Session",
            "StartExam",
            "ResumeExam"
        };

        var methodNames = typeof(PackagePracticeProgress)
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name);

        Assert.All(methodNames, methodName =>
        {
            Assert.DoesNotContain(forbiddenMethodFragments, fragment =>
                methodName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static PackagePracticeProgress CreateProgress(ProgressFacts? facts = null)
    {
        facts ??= CreateProgressFacts(isCorrect: true);

        return PackagePracticeProgress.Create(
            facts.NurseProfileId,
            facts.PackagePurchaseEntitlementId,
            facts.PracticeCollectionVersionId,
            facts.PracticeItemId,
            facts.SelectedPracticeAnswerOptionId,
            facts.IsCorrect,
            facts.AnsweredAt);
    }

    private static ProgressFacts CreateProgressFacts(bool isCorrect)
    {
        return new ProgressFacts(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            isCorrect,
            new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc));
    }

    private sealed record ProgressFacts(
        Guid NurseProfileId,
        Guid PackagePurchaseEntitlementId,
        Guid PracticeCollectionVersionId,
        Guid PracticeItemId,
        Guid SelectedPracticeAnswerOptionId,
        bool IsCorrect,
        DateTime AnsweredAt);
}
