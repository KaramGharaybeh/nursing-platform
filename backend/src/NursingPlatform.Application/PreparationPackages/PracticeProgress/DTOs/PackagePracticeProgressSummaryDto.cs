namespace NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

public sealed class PackagePracticeProgressSummaryDto
{
    public Guid PackagePurchaseEntitlementId { get; init; }
    public Guid PracticeCollectionVersionId { get; init; }
    public int TotalItems { get; init; }
    public int AnsweredCount { get; init; }
    public int UnansweredCount { get; init; }
    public int CorrectCount { get; init; }
    public int IncorrectCount { get; init; }
    public IReadOnlyList<PackagePracticeProgressItemStateDto> ItemStates { get; init; } = [];
}
