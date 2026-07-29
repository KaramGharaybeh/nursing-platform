namespace NursingPlatform.Application.PreparationPackages.Entitlements.DTOs;

public class PackageEntitlementListItemDto
{
    public Guid Id { get; set; }
    public Guid PackageOfferId { get; set; }
    public string PackageOfferTitle { get; set; } = string.Empty;
    public Guid PackageDefinitionId { get; set; }
    public string PackageDefinitionTitle { get; set; } = string.Empty;
    public Guid PackageVersionId { get; set; }
    public Guid IncludedExamId { get; set; }
    public string IncludedExamTitle { get; set; } = string.Empty;
    public DateTime AccessStartsAt { get; set; }
    public DateTime AccessEndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PackageEntitlementDetailDto : PackageEntitlementListItemDto
{
    public PackageEntitlementSnapshotDto PurchasedSnapshot { get; set; } = new();
    public List<PackageBenefitRightSummaryDto> BenefitRights { get; set; } = [];
}

public class PackageEntitlementSnapshotDto
{
    public string PackageOfferSlug { get; set; } = string.Empty;
    public string? PackageOfferSummary { get; set; }
    public string PackageDefinitionSlug { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public Guid ExamCategoryId { get; set; }
    public Guid IncludedExamVersionId { get; set; }
    public Guid ReportingProfilePublicationId { get; set; }
    public Guid PracticeCollectionVersionId { get; set; }
    public List<Guid> StudyMaterialVersionIds { get; set; } = [];
    public long PriceAmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int AccessDurationDays { get; set; }
}

public class PackageBenefitRightSummaryDto
{
    public string RightType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AccessStartsAt { get; set; }
    public DateTime? AccessEndsAt { get; set; }
}
