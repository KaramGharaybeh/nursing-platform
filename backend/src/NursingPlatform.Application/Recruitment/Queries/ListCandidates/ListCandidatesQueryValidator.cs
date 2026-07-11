using FluentValidation;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

public class ListCandidatesQueryValidator : AbstractValidator<ListCandidatesQuery>
{
    public ListCandidatesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.MinimumYearsOfExperience)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumYearsOfExperience.HasValue)
            .WithMessage("Minimum years of experience must be at least 0.");

        RuleFor(x => x.Skills)
            .Must(NotContainBlankSkillValues)
            .WithMessage("Skill name is required.")
            .Must(NotExceedMaximumNormalizedSkillCount)
            .WithMessage("At most 20 skill filters are allowed.")
            .Must(NotContainOverLengthSkillValues)
            .WithMessage("Skill name must not exceed 100 characters.");
    }

    private static bool NotContainBlankSkillValues(IReadOnlyCollection<string> skills)
    {
        return CandidateSkillFilterParser.ParseDisplayNames(skills)
            .All(skill => !string.IsNullOrWhiteSpace(skill));
    }

    private static bool NotExceedMaximumNormalizedSkillCount(IReadOnlyCollection<string> skills)
    {
        return CandidateSkillFilterParser.ParseNormalizedNames(skills).Count <= CandidateSkillFilterParser.MaxNormalizedSkillCount;
    }

    private static bool NotContainOverLengthSkillValues(IReadOnlyCollection<string> skills)
    {
        return CandidateSkillFilterParser.ParseDisplayNames(skills)
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(SkillNameNormalizer.NormalizeName)
            .All(skill => skill.Length <= CandidateSkillFilterParser.MaxSkillNameLength);
    }
}
