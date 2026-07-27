using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class ReportingTopic : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid ExamCategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static ReportingTopic Create(Guid examCategoryId, string name, string slug, string? description)
    {
        if (examCategoryId == Guid.Empty)
        {
            throw new InvalidOperationException("examCategoryId is required.");
        }

        return new ReportingTopic
        {
            Id = Guid.NewGuid(),
            ExamCategoryId = examCategoryId,
            Name = NormalizeRequired(name, nameof(name)),
            Slug = NormalizeRequired(slug, nameof(slug)),
            Description = NormalizeOptional(description)
        };
    }

    public void Archive()
    {
        IsActive = false;
    }

    public void Update(Guid examCategoryId, string name, string slug, string? description)
    {
        if (examCategoryId == Guid.Empty)
        {
            throw new InvalidOperationException("examCategoryId is required.");
        }

        ExamCategoryId = examCategoryId;
        Name = NormalizeRequired(name, nameof(name));
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
}
