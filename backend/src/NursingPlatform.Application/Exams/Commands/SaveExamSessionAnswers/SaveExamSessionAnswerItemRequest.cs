namespace NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;

public class SaveExamSessionAnswerItemRequest
{
    public Guid ExamSessionQuestionId { get; set; }
    public Guid SelectedExamSessionAnswerOptionId { get; set; }
}
