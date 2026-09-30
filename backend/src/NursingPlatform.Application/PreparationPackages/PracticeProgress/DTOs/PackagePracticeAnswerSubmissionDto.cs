namespace NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

public sealed class PackagePracticeAnswerSubmissionDto
{
    public Guid PracticeItemId { get; init; }
    public PackagePracticeProgressItemState State { get; init; }
    public Guid SelectedPracticeAnswerOptionId { get; init; }
    public DateTime LastAnsweredAt { get; init; }
    public string ImmediateFeedback { get; init; } = string.Empty;
}
