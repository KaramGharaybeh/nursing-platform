using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PreparationPackageDefinition : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid CountryId { get; private set; }
    public Guid ExamCategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static PreparationPackageDefinition Create(
        Guid countryId,
        Guid examCategoryId,
        string title,
        string slug,
        string? description)
    {
        EnsureNotEmpty(countryId, nameof(countryId));
        EnsureNotEmpty(examCategoryId, nameof(examCategoryId));

        return new PreparationPackageDefinition
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            ExamCategoryId = examCategoryId,
            Title = NormalizeRequired(title, nameof(title)),
            Slug = NormalizeRequired(slug, nameof(slug)),
            Description = NormalizeOptional(description)
        };
    }

    public void Update(
        Guid countryId,
        Guid examCategoryId,
        string title,
        string slug,
        string? description)
    {
        EnsureNotEmpty(countryId, nameof(countryId));
        EnsureNotEmpty(examCategoryId, nameof(examCategoryId));

        CountryId = countryId;
        ExamCategoryId = examCategoryId;
        Title = NormalizeRequired(title, nameof(title));
        Slug = NormalizeRequired(slug, nameof(slug));
        Description = NormalizeOptional(description);
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
