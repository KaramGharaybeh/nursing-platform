namespace NursingPlatform.Application.Exams.Common;

using NursingPlatform.Domain.Exams;

public interface IExamAccessPolicy
{
    Task AuthorizeStartAsync(Guid nurseProfileId, Guid examId, CancellationToken cancellationToken);
    Task<ExamSessionSource> AuthorizeStartAndGetSourceAsync(Guid nurseProfileId, Guid examId, CancellationToken cancellationToken);
}
