using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PracticeCollection : AuditableEntity
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static PracticeCollection Create(string title, string slug, string? description)
    {
        return new PracticeCollection
        {
            Id = Guid.NewGuid(),
            Title = NormalizeRequired(title, nameof(title)),
            Slug = NormalizeRequired(slug, nameof(slug)),
            Description = NormalizeOptional(description)
        };
    }

    public void Archive()
    {
        IsActive = false;
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
}
