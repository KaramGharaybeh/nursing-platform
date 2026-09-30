namespace NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

public sealed class PackagePracticeProgressItemStateDto
{
    public Guid PracticeItemId { get; init; }
    public PackagePracticeProgressItemState State { get; init; }
    public Guid? SelectedPracticeAnswerOptionId { get; init; }
    public DateTime? LastAnsweredAt { get; init; }
}
