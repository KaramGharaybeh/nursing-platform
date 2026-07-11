using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

internal static class CandidateSkillFilterParser
{
    public const int MaxNormalizedSkillCount = 20;
    public const int MaxSkillNameLength = 100;

    public static IReadOnlyList<string> ParseDisplayNames(IReadOnlyCollection<string>? skills)
    {
        if (skills is null)
        {
            return [];
        }

        return skills
            .SelectMany(skill => (skill ?? string.Empty).Split(',', StringSplitOptions.None))
            .Select(skill => skill.Trim())
            .ToList();
    }

    public static IReadOnlyList<string> ParseNormalizedNames(IReadOnlyCollection<string>? skills)
    {
        return ParseDisplayNames(skills)
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(SkillNameNormalizer.NormalizeName)
            .Select(SkillNameNormalizer.NormalizeForComparison)
            .Distinct()
            .ToList();
    }
}
