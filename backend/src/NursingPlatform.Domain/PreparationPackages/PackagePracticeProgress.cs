using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PackagePracticeProgress : AuditableEntity
{
    private PackagePracticeProgress()
    {
    }

    public Guid Id { get; private set; }
    public Guid NurseProfileId { get; private set; }
    public Guid PackagePurchaseEntitlementId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public Guid PracticeItemId { get; private set; }
    public Guid SelectedPracticeAnswerOptionId { get; private set; }
    public PackagePracticeProgressState State { get; private set; }
    public bool IsCorrect { get; private set; }
    public DateTime LastAnsweredAt { get; private set; }

    public static PackagePracticeProgress Create(
        Guid nurseProfileId,
        Guid packagePurchaseEntitlementId,
        Guid practiceCollectionVersionId,
        Guid practiceItemId,
        Guid selectedPracticeAnswerOptionId,
        bool isCorrect,
        DateTime answeredAt)
    {
        RequireNotEmpty(nurseProfileId, nameof(nurseProfileId));
        RequireNotEmpty(packagePurchaseEntitlementId, nameof(packagePurchaseEntitlementId));
        RequireNotEmpty(practiceCollectionVersionId, nameof(practiceCollectionVersionId));
        RequireNotEmpty(practiceItemId, nameof(practiceItemId));
        RequireNotEmpty(selectedPracticeAnswerOptionId, nameof(selectedPracticeAnswerOptionId));
        RequireUtc(answeredAt, nameof(answeredAt));

        return new PackagePracticeProgress
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            PackagePurchaseEntitlementId = packagePurchaseEntitlementId,
            PracticeCollectionVersionId = practiceCollectionVersionId,
            PracticeItemId = practiceItemId,
            SelectedPracticeAnswerOptionId = selectedPracticeAnswerOptionId,
            IsCorrect = isCorrect,
            State = ToState(isCorrect),
            LastAnsweredAt = answeredAt,
            CreatedAt = answeredAt,
            UpdatedAt = answeredAt
        };
    }

    public void UpdateAnswer(
        Guid selectedPracticeAnswerOptionId,
        bool isCorrect,
        DateTime answeredAt)
    {
        RequireNotEmpty(selectedPracticeAnswerOptionId, nameof(selectedPracticeAnswerOptionId));
        RequireUtc(answeredAt, nameof(answeredAt));

        SelectedPracticeAnswerOptionId = selectedPracticeAnswerOptionId;
        IsCorrect = isCorrect;
        State = ToState(isCorrect);
        LastAnsweredAt = answeredAt;
        UpdatedAt = answeredAt;
    }

    private static PackagePracticeProgressState ToState(bool isCorrect)
    {
        return isCorrect
            ? PackagePracticeProgressState.AnsweredCorrect
            : PackagePracticeProgressState.AnsweredIncorrect;
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }

    private static void RequireUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{name} must be UTC.");
        }
    }
}
