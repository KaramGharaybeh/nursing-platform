using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PreparationPackageVersion : AuditableEntity
{
    private readonly List<PreparationPackageVersionMaterial> _materials = [];

    public Guid Id { get; private set; }
    public Guid PreparationPackageDefinitionId { get; private set; }
    public Guid ExamVersionId { get; private set; }
    public Guid ReportingProfilePublicationId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public PreparationPackageVersionStatus Status { get; private set; } = PreparationPackageVersionStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }
    public bool ContentIsolationConfirmed { get; private set; }
    public IReadOnlyCollection<PreparationPackageVersionMaterial> Materials => _materials.AsReadOnly();

    public static PreparationPackageVersion CreateDraft(
        Guid preparationPackageDefinitionId,
        Guid examVersionId,
        Guid reportingProfilePublicationId,
        Guid practiceCollectionVersionId)
    {
        EnsureNotEmpty(preparationPackageDefinitionId, nameof(preparationPackageDefinitionId));
        EnsureNotEmpty(examVersionId, nameof(examVersionId));
        EnsureNotEmpty(reportingProfilePublicationId, nameof(reportingProfilePublicationId));
        EnsureNotEmpty(practiceCollectionVersionId, nameof(practiceCollectionVersionId));

        return new PreparationPackageVersion
        {
            Id = Guid.NewGuid(),
            PreparationPackageDefinitionId = preparationPackageDefinitionId,
            ExamVersionId = examVersionId,
            ReportingProfilePublicationId = reportingProfilePublicationId,
            PracticeCollectionVersionId = practiceCollectionVersionId
        };
    }

    public void AddMaterialVersion(Guid studyMaterialVersionId, int sortOrder)
    {
        EnsureDraft();
        EnsureNotEmpty(studyMaterialVersionId, nameof(studyMaterialVersionId));

        if (_materials.Any(material => material.SortOrder == sortOrder))
        {
            throw new InvalidOperationException("Material sort order must be unique within a package version.");
        }

        var material = PreparationPackageVersionMaterial.Create(studyMaterialVersionId, sortOrder);
        material.PreparationPackageVersionId = Id;
        _materials.Add(material);
    }

    public IReadOnlyList<PreparationPackageVersionMaterial> GetOrderedMaterials()
    {
        return _materials.OrderBy(material => material.SortOrder).ToList();
    }

    public void ConfirmContentIsolation()
    {
        EnsureDraft();
        ContentIsolationConfirmed = true;
    }

    public void Publish(DateTime publishedAt)
    {
        EnsureDraft();

        if (_materials.Count == 0)
        {
            throw new InvalidOperationException("A package version must include at least one material version before publication.");
        }

        if (!ContentIsolationConfirmed)
        {
            throw new InvalidOperationException("Package version content isolation must be confirmed before publication.");
        }

        Status = PreparationPackageVersionStatus.Published;
        PublishedAt = publishedAt;
    }

    public void Retire(DateTime retiredAt)
    {
        if (Status != PreparationPackageVersionStatus.Published)
        {
            throw new InvalidOperationException("Only published package versions can be retired.");
        }

        Status = PreparationPackageVersionStatus.Retired;
        RetiredAt = retiredAt;
    }

    private void EnsureDraft()
    {
        if (Status != PreparationPackageVersionStatus.Draft)
        {
            throw new InvalidOperationException("Published or retired package versions are immutable.");
        }
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{parameterName} is required.");
        }
    }
}
