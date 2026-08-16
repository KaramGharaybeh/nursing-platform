using System.Text.Json.Serialization;

namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string Currency { get; set; } = string.Empty;
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long UnitAmountMinor { get; set; }
    public int Quantity { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long LineTotalAmountMinor { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public PaymentPackageSnapshotDto? PackageSnapshot { get; set; }
}

public class PaymentPackageSnapshotDto
{
    public Guid PackageOfferId { get; set; }
    public string PackageOfferTitle { get; set; } = string.Empty;
    public string PackageOfferSlug { get; set; } = string.Empty;
    public string? PackageOfferSummary { get; set; }
    public Guid PackageDefinitionId { get; set; }
    public string PackageDefinitionTitle { get; set; } = string.Empty;
    public string PackageDefinitionSlug { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public Guid ExamCategoryId { get; set; }
    public Guid PackageVersionId { get; set; }
    public int PackageVersionNumber { get; set; }
    public Guid IncludedExamId { get; set; }
    public Guid IncludedExamVersionId { get; set; }
    public string IncludedExamTitle { get; set; } = string.Empty;
    public Guid ReportingProfilePublicationId { get; set; }
    public Guid PracticeCollectionVersionId { get; set; }
    public List<Guid> StudyMaterialVersionIds { get; set; } = [];
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long PriceAmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int AccessDurationDays { get; set; }
    public DateTime OrderCreatedAt { get; set; }
}
