using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PreparationPackageVersionMaterial : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PreparationPackageVersionId { get; internal set; }
    public Guid StudyMaterialVersionId { get; private set; }
    public int SortOrder { get; private set; }

    internal static PreparationPackageVersionMaterial Create(Guid studyMaterialVersionId, int sortOrder)
    {
        if (studyMaterialVersionId == Guid.Empty)
        {
            throw new InvalidOperationException("studyMaterialVersionId is required.");
        }

        if (sortOrder < 1)
        {
            throw new InvalidOperationException("Material sort order must be positive.");
        }

        return new PreparationPackageVersionMaterial
        {
            Id = Guid.NewGuid(),
            StudyMaterialVersionId = studyMaterialVersionId,
            SortOrder = sortOrder
        };
    }
}
