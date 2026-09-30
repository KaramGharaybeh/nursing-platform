using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PreparationPackageOffer : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PreparationPackageDefinitionId { get; private set; }
    public Guid PreparationPackageVersionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public long PriceAmountMinor { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int AccessDurationDays { get; private set; }
    public PreparationPackageOfferStatus Status { get; private set; } = PreparationPackageOfferStatus.Draft;
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }

    public static PreparationPackageOffer CreateDraft(
        Guid preparationPackageDefinitionId,
        Guid preparationPackageVersionId,
        string title,
        string slug,
        string? summary,
        long priceAmountMinor,
        string currency,
        int accessDurationDays)
    {
        EnsureNotEmpty(preparationPackageDefinitionId, nameof(preparationPackageDefinitionId));
        EnsureNotEmpty(preparationPackageVersionId, nameof(preparationPackageVersionId));

        if (priceAmountMinor < 0)
        {
            throw new InvalidOperationException("Package offer price cannot be negative.");
        }

        if (accessDurationDays < 1)
        {
            throw new InvalidOperationException("Package offer access duration must be positive.");
        }

        return new PreparationPackageOffer
        {
            Id = Guid.NewGuid(),
            PreparationPackageDefinitionId = preparationPackageDefinitionId,
            PreparationPackageVersionId = preparationPackageVersionId,
            Title = NormalizeRequired(title, nameof(title)),
            Slug = NormalizeRequired(slug, nameof(slug)),
            Summary = NormalizeOptional(summary),
            PriceAmountMinor = priceAmountMinor,
            Currency = NormalizeRequired(currency, nameof(currency)).ToUpperInvariant(),
            AccessDurationDays = accessDurationDays
        };
    }

    public void Activate(DateTime activatedAt)
    {
        if (Status is PreparationPackageOfferStatus.Retired)
        {
            throw new InvalidOperationException("Retired package offers cannot be activated.");
        }

        Status = PreparationPackageOfferStatus.Active;
        ActivatedAt = activatedAt;
        DeactivatedAt = null;
    }

    public void UpdateDraft(
        Guid preparationPackageDefinitionId,
        Guid preparationPackageVersionId,
        string title,
        string slug,
        string? summary,
        long priceAmountMinor,
        string currency,
        int accessDurationDays)
    {
        if (Status != PreparationPackageOfferStatus.Draft)
        {
            throw new InvalidOperationException("Only draft package offers can be updated.");
        }

        EnsureNotEmpty(preparationPackageDefinitionId, nameof(preparationPackageDefinitionId));
        EnsureNotEmpty(preparationPackageVersionId, nameof(preparationPackageVersionId));

        if (priceAmountMinor < 0)
        {
            throw new InvalidOperationException("Package offer price cannot be negative.");
        }

        if (accessDurationDays < 1)
        {
            throw new InvalidOperationException("Package offer access duration must be positive.");
        }

        PreparationPackageDefinitionId = preparationPackageDefinitionId;
        PreparationPackageVersionId = preparationPackageVersionId;
        Title = NormalizeRequired(title, nameof(title));
        Slug = NormalizeRequired(slug, nameof(slug));
        Summary = NormalizeOptional(summary);
        PriceAmountMinor = priceAmountMinor;
        Currency = NormalizeRequired(currency, nameof(currency)).ToUpperInvariant();
        AccessDurationDays = accessDurationDays;
    }

    public void Deactivate(DateTime deactivatedAt)
    {
        if (Status != PreparationPackageOfferStatus.Active)
        {
            throw new InvalidOperationException("Only active package offers can be deactivated.");
        }

        Status = PreparationPackageOfferStatus.Inactive;
        DeactivatedAt = deactivatedAt;
    }

    public void Retire(DateTime retiredAt)
    {
        Status = PreparationPackageOfferStatus.Retired;
        RetiredAt = retiredAt;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{parameterName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{parameterName} is required.");
        }
    }
}
