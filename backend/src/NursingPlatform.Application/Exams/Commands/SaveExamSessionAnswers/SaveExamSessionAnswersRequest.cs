namespace NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;

public class SaveExamSessionAnswersRequest
{
    public IReadOnlyList<SaveExamSessionAnswerItemRequest> Answers { get; set; } = [];
}
