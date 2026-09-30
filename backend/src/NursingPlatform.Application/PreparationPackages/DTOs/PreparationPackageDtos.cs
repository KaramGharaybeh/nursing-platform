using System.Text.Json.Serialization;

namespace NursingPlatform.Application.PreparationPackages.DTOs;

public class PreparationPackageCatalogComponentSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? Summary { get; set; }
}

public class PreparationPackageOfferListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public Guid ExamCategoryId { get; set; }
    public string ExamCategoryName { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int MaterialCount { get; set; }
    public int PracticeItemCount { get; set; }
    public int AccessDurationDays { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long PriceAmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class PreparationPackageOfferDetailDto : PreparationPackageOfferListItemDto
{
    public List<PreparationPackageCatalogComponentSummaryDto> Components { get; set; } = [];
}

public class AdminReportingTopicDto
{
    public Guid Id { get; set; }
    public Guid ExamCategoryId { get; set; }
    public string ExamCategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AdminReportingProfilePublicationDto
{
    public Guid Id { get; set; }
    public Guid ExamVersionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public List<AdminReportingProfileQuestionAssignmentDto> Assignments { get; set; } = [];
}

public class AdminReportingProfileQuestionAssignmentDto
{
    public Guid ExamQuestionId { get; set; }
    public Guid ReportingTopicId { get; set; }
    public string ReportingTopicName { get; set; } = string.Empty;
}

public class AdminStudyMaterialDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AdminStudyMaterialVersionDto
{
    public Guid Id { get; set; }
    public Guid StudyMaterialId { get; set; }
    public int VersionNumber { get; set; }
    public string MaterialType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FormattedTextContent { get; set; }
    public string? FileStorageKey { get; set; }
    public string? ExternalUrl { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<Guid> ReportingTopicIds { get; set; } = [];
}

public class AdminPracticeCollectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AdminPracticeCollectionVersionDto
{
    public Guid Id { get; set; }
    public Guid PracticeCollectionId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public List<AdminPracticeItemDto> Items { get; set; } = [];
}

public class AdminPracticeItemDto
{
    public Guid Id { get; set; }
    public Guid ReportingTopicId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string ImmediateFeedback { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<AdminPracticeAnswerOptionDto> Options { get; set; } = [];
}

public class AdminPracticeAnswerOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCorrect { get; set; }
}

public class AdminPreparationPackageDefinitionDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public Guid ExamCategoryId { get; set; }
    public string ExamCategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AdminPreparationPackageVersionDto
{
    public Guid Id { get; set; }
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid ExamVersionId { get; set; }
    public Guid ReportingProfilePublicationId { get; set; }
    public Guid PracticeCollectionVersionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool ContentIsolationConfirmed { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<PreparationPackageVersionMaterialDto> Materials { get; set; } = [];
}

public class PreparationPackageVersionMaterialDto
{
    public Guid StudyMaterialVersionId { get; set; }
    public int SortOrder { get; set; }
}

public class AdminPreparationPackageOfferDto
{
    public Guid Id { get; set; }
    public Guid PreparationPackageDefinitionId { get; set; }
    public Guid PreparationPackageVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long PriceAmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int AccessDurationDays { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PackagePublicationValidationDto
{
    public bool IsValid => Issues.Count == 0;
    public List<PackagePublicationValidationIssueDto> Issues { get; set; } = [];
}

public class PackagePublicationValidationIssueDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
