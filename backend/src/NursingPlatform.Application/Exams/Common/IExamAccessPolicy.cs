namespace NursingPlatform.Application.Exams.Common;

public interface IExamAccessPolicy
{
    Task AuthorizeStartAsync(Guid nurseProfileId, Guid examId, CancellationToken cancellationToken);
}
