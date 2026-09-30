namespace NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

public sealed class PackagePracticeContentListDto
{
    public Guid PackagePurchaseEntitlementId { get; init; }
    public Guid PracticeCollectionVersionId { get; init; }
    public int TotalItems { get; init; }
    public IReadOnlyList<PackagePracticeItemContentDto> Items { get; init; } = [];
}

public sealed class PackagePracticeItemContentDto
{
    public Guid PracticeItemId { get; init; }
    public int DisplayOrder { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<PackagePracticeAnswerOptionContentDto> AnswerOptions { get; init; } = [];
}

public sealed class PackagePracticeAnswerOptionContentDto
{
    public Guid PracticeAnswerOptionId { get; init; }
    public string OptionText { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
