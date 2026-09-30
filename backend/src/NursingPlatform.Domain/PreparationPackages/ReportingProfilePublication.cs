using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class ReportingProfilePublication : AuditableEntity
{
    private readonly List<ReportingProfileQuestionAssignment> _assignments = [];

    public Guid Id { get; private set; }
    public Guid ExamVersionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PublicationStatus Status { get; private set; } = PublicationStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }
    public IReadOnlyCollection<ReportingProfileQuestionAssignment> Assignments => _assignments.AsReadOnly();

    public static ReportingProfilePublication CreateDraft(Guid examVersionId, string name)
    {
        if (examVersionId == Guid.Empty)
        {
            throw new InvalidOperationException("examVersionId is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Reporting profile name is required.");
        }

        return new ReportingProfilePublication
        {
            Id = Guid.NewGuid(),
            ExamVersionId = examVersionId,
            Name = name.Trim()
        };
    }

    public void AssignQuestion(Guid examQuestionId, Guid reportingTopicId)
    {
        EnsureDraft();

        if (examQuestionId == Guid.Empty)
        {
            throw new InvalidOperationException("examQuestionId is required.");
        }

        if (reportingTopicId == Guid.Empty)
        {
            throw new InvalidOperationException("reportingTopicId is required.");
        }

        if (_assignments.Any(assignment => assignment.ExamQuestionId == examQuestionId))
        {
            throw new InvalidOperationException("Each exam question can have only one reporting topic assignment in a profile publication.");
        }

        var assignment = ReportingProfileQuestionAssignment.Create(examQuestionId, reportingTopicId);
        assignment.ReportingProfilePublicationId = Id;
        _assignments.Add(assignment);
    }

    public void Publish(DateTime publishedAt)
    {
        EnsureDraft();

        if (_assignments.Count == 0)
        {
            throw new InvalidOperationException("A reporting profile publication must include at least one question assignment before publication.");
        }

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
    }

    public void Retire(DateTime retiredAt)
    {
        if (Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException("Only published reporting profiles can be retired.");
        }

        Status = PublicationStatus.Retired;
        RetiredAt = retiredAt;
    }

    private void EnsureDraft()
    {
        if (Status != PublicationStatus.Draft)
        {
            throw new InvalidOperationException("Published or retired reporting profile publications are immutable.");
        }
    }
}
