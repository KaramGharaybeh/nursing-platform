using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class ReportingProfileQuestionAssignment : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid ReportingProfilePublicationId { get; internal set; }
    public Guid ExamQuestionId { get; private set; }
    public Guid ReportingTopicId { get; private set; }

    internal static ReportingProfileQuestionAssignment Create(Guid examQuestionId, Guid reportingTopicId)
    {
        if (examQuestionId == Guid.Empty)
        {
            throw new InvalidOperationException("examQuestionId is required.");
        }

        if (reportingTopicId == Guid.Empty)
        {
            throw new InvalidOperationException("reportingTopicId is required.");
        }

        return new ReportingProfileQuestionAssignment
        {
            Id = Guid.NewGuid(),
            ExamQuestionId = examQuestionId,
            ReportingTopicId = reportingTopicId
        };
    }
}
