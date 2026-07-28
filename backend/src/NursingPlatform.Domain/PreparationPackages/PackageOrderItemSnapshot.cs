using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PackageOrderItemSnapshot : AuditableEntity
{
    private readonly List<Guid> _studyMaterialVersionIds = [];

    private PackageOrderItemSnapshot()
    {
    }

    public Guid Id { get; private set; }
    public Guid PaymentOrderItemId { get; private set; }
    public Guid PackageOfferId { get; private set; }
    public string PackageOfferTitle { get; private set; } = string.Empty;
    public string PackageOfferSlug { get; private set; } = string.Empty;
    public string? PackageOfferSummary { get; private set; }
    public Guid PackageDefinitionId { get; private set; }
    public string PackageDefinitionTitle { get; private set; } = string.Empty;
    public string PackageDefinitionSlug { get; private set; } = string.Empty;
    public Guid CountryId { get; private set; }
    public Guid ExamCategoryId { get; private set; }
    public Guid PackageVersionId { get; private set; }
    public int PackageVersionNumber { get; private set; }
    public Guid IncludedExamId { get; private set; }
    public Guid IncludedExamVersionId { get; private set; }
    public string IncludedExamTitle { get; private set; } = string.Empty;
    public Guid ReportingProfilePublicationId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public IReadOnlyList<Guid> StudyMaterialVersionIds => _studyMaterialVersionIds.AsReadOnly();
    public long PriceAmountMinor { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int AccessDurationDays { get; private set; }
    public DateTime OrderCreatedAt { get; private set; }

    public static PackageOrderItemSnapshot Create(
        Guid packageOfferId,
        string packageOfferTitle,
        string packageOfferSlug,
        string? packageOfferSummary,
        Guid packageDefinitionId,
        string packageDefinitionTitle,
        string packageDefinitionSlug,
        Guid countryId,
        Guid examCategoryId,
        Guid packageVersionId,
        int packageVersionNumber,
        Guid includedExamId,
        Guid includedExamVersionId,
        string includedExamTitle,
        Guid reportingProfilePublicationId,
        Guid practiceCollectionVersionId,
        IReadOnlyCollection<Guid> studyMaterialVersionIds,
        long priceAmountMinor,
        string currency,
        int accessDurationDays,
        DateTime orderCreatedAt)
    {
        RequireNotEmpty(packageOfferId, nameof(packageOfferId));
        RequireNotEmpty(packageDefinitionId, nameof(packageDefinitionId));
        RequireNotEmpty(countryId, nameof(countryId));
        RequireNotEmpty(examCategoryId, nameof(examCategoryId));
        RequireNotEmpty(packageVersionId, nameof(packageVersionId));
        RequireNotEmpty(includedExamId, nameof(includedExamId));
        RequireNotEmpty(includedExamVersionId, nameof(includedExamVersionId));
        RequireNotEmpty(reportingProfilePublicationId, nameof(reportingProfilePublicationId));
        RequireNotEmpty(practiceCollectionVersionId, nameof(practiceCollectionVersionId));

        if (packageVersionNumber <= 0)
        {
            throw new InvalidOperationException("Package version number must be positive.");
        }

        if (priceAmountMinor < 0)
        {
            throw new InvalidOperationException("Package snapshot price cannot be negative.");
        }

        if (accessDurationDays <= 0)
        {
            throw new InvalidOperationException("Package access duration must be positive.");
        }

        if (orderCreatedAt.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Package snapshot order creation timestamp must be UTC.");
        }

        if (studyMaterialVersionIds.Count == 0 || studyMaterialVersionIds.Any(id => id == Guid.Empty))
        {
            throw new InvalidOperationException("Package snapshot must include non-empty study material version ids.");
        }

        var snapshot = new PackageOrderItemSnapshot
        {
            Id = Guid.NewGuid(),
            PackageOfferId = packageOfferId,
            PackageOfferTitle = RequireText(packageOfferTitle, nameof(packageOfferTitle)),
            PackageOfferSlug = RequireText(packageOfferSlug, nameof(packageOfferSlug)),
            PackageOfferSummary = string.IsNullOrWhiteSpace(packageOfferSummary) ? null : packageOfferSummary.Trim(),
            PackageDefinitionId = packageDefinitionId,
            PackageDefinitionTitle = RequireText(packageDefinitionTitle, nameof(packageDefinitionTitle)),
            PackageDefinitionSlug = RequireText(packageDefinitionSlug, nameof(packageDefinitionSlug)),
            CountryId = countryId,
            ExamCategoryId = examCategoryId,
            PackageVersionId = packageVersionId,
            PackageVersionNumber = packageVersionNumber,
            IncludedExamId = includedExamId,
            IncludedExamVersionId = includedExamVersionId,
            IncludedExamTitle = RequireText(includedExamTitle, nameof(includedExamTitle)),
            ReportingProfilePublicationId = reportingProfilePublicationId,
            PracticeCollectionVersionId = practiceCollectionVersionId,
            PriceAmountMinor = priceAmountMinor,
            Currency = RequireText(currency, nameof(currency)).ToUpperInvariant(),
            AccessDurationDays = accessDurationDays,
            OrderCreatedAt = orderCreatedAt
        };

        snapshot._studyMaterialVersionIds.AddRange(studyMaterialVersionIds);
        return snapshot;
    }

    public void AssignPaymentOrderItem(Guid paymentOrderItemId)
    {
        RequireNotEmpty(paymentOrderItemId, nameof(paymentOrderItemId));

        if (PaymentOrderItemId != Guid.Empty && PaymentOrderItemId != paymentOrderItemId)
        {
            throw new InvalidOperationException("Package order item snapshot payment order item cannot be changed.");
        }

        PaymentOrderItemId = paymentOrderItemId;
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }

    private static string RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        return value.Trim();
    }
}
