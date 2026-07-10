using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

public class UpdateNurseSkillsCommandValidator : AbstractValidator<UpdateNurseSkillsCommand>
{
    public UpdateNurseSkillsCommandValidator()
    {
        RuleFor(x => x.Skills)
            .Must(skills => skills.Count <= 50)
            .WithMessage("A nurse can have at most 50 skills.");

        RuleFor(x => x.Skills)
            .Must(NotContainNormalizedDuplicates)
            .WithMessage("Duplicate skill names are not allowed.");

        RuleForEach(x => x.Skills)
            .Must(skill => !string.IsNullOrWhiteSpace(skill))
            .WithMessage("Skill name is required.");
    }

    private static bool NotContainNormalizedDuplicates(IReadOnlyCollection<string> skills)
    {
        var normalizedNames = skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(SkillNameNormalizer.NormalizeName)
            .Select(SkillNameNormalizer.NormalizeForComparison)
            .ToList();

        return normalizedNames.Distinct().Count() == normalizedNames.Count;
    }
}

public static class SkillNameNormalizer
{
    public static string NormalizeName(string value)
    {
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string NormalizeForComparison(string value)
    {
        return NormalizeName(value).ToUpperInvariant();
    }
}
