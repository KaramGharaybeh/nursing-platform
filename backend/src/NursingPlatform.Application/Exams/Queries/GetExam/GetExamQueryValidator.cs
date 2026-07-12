using FluentValidation;

namespace NursingPlatform.Application.Exams.Queries.GetExam;

public class GetExamQueryValidator : AbstractValidator<GetExamQuery>
{
    public GetExamQueryValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
    }
}
