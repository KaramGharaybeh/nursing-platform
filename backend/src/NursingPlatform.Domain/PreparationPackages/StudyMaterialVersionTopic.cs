using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class StudyMaterialVersionTopic : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid StudyMaterialVersionId { get; internal set; }
    public Guid ReportingTopicId { get; private set; }

    internal static StudyMaterialVersionTopic Create(Guid reportingTopicId)
    {
        if (reportingTopicId == Guid.Empty)
        {
            throw new InvalidOperationException("reportingTopicId is required.");
        }

        return new StudyMaterialVersionTopic
        {
            Id = Guid.NewGuid(),
            ReportingTopicId = reportingTopicId
        };
    }
}
