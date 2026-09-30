namespace NursingPlatform.Application.Payments.DTOs;

public class PaymentCompletionDto
{
    public Guid PaymentOrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public IReadOnlyList<Guid> GrantedExamIds { get; set; } = [];
    public IReadOnlyList<PaymentPackageEntitlementSummaryDto> PackageEntitlements { get; set; } = [];
}

public class PaymentPackageEntitlementSummaryDto
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
