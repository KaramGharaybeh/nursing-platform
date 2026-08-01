using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Common;

internal sealed class PreparationPackagePublicationValidator
{
    private readonly IApplicationDbContext _context;

    public PreparationPackagePublicationValidator(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PackagePublicationValidationDto> ValidatePackageVersionAsync(
        PreparationPackageVersion version,
        CancellationToken cancellationToken)
    {
        var issues = new List<PackagePublicationValidationIssueDto>();
        var definition = await _context.PreparationPackageDefinitions.FirstOrDefaultAsync(d => d.Id == version.PreparationPackageDefinitionId, cancellationToken);
        if (definition is null)
        {
            Add(issues, "PackageDefinitionMissing", "Package definition was not found.");
            return ToDto(issues);
        }

        var examVersion = await _context.ExamVersions.FirstOrDefaultAsync(v => v.Id == version.ExamVersionId, cancellationToken);
        if (examVersion is null || examVersion.Status != ExamVersionStatus.Published)
        {
            Add(issues, "ExamVersionNotPublished", "Referenced exam version must be published.");
        }

        var exam = examVersion is null
            ? null
            : await _context.Exams.FirstOrDefaultAsync(e => e.Id == examVersion.ExamId, cancellationToken);
        if (exam is null || exam.CountryId != definition.CountryId || exam.ExamCategoryId != definition.ExamCategoryId)
        {
            Add(issues, "ExamContextMismatch", "Referenced exam version must match the package country and exam category context.");
        }

        var profile = await _context.ReportingProfilePublications.FirstOrDefaultAsync(p => p.Id == version.ReportingProfilePublicationId, cancellationToken);
        if (profile is null || profile.Status != PublicationStatus.Published)
        {
            Add(issues, "ReportingProfileNotPublished", "Referenced reporting profile must be published.");
        }
        else
        {
            if (profile.ExamVersionId != version.ExamVersionId)
            {
                Add(issues, "ReportingProfileExamVersionMismatch", "Reporting profile must be bound to the exact referenced exam version.");
            }

            if (!profile.Assignments.Any())
            {
                Add(issues, "ReportingProfileAssignmentsMissing", "Reporting profile must include question assignments.");
            }
        }

        var practiceVersion = await _context.PracticeCollectionVersions.FirstOrDefaultAsync(v => v.Id == version.PracticeCollectionVersionId, cancellationToken);
        if (practiceVersion is null || practiceVersion.Status != PublicationStatus.Published)
        {
            Add(issues, "PracticeCollectionNotPublished", "Referenced practice collection version must be published.");
        }
        else
        {
            if (!practiceVersion.Items.Any())
            {
                Add(issues, "PracticeCollectionItemsMissing", "Practice collection version must include practice items.");
            }

            foreach (var item in practiceVersion.Items)
            {
                if (item.ReportingTopicId == Guid.Empty)
                {
                    Add(issues, "PracticeItemTopicMissing", "Every practice item must map to a reporting topic.");
                }

                if (item.AnswerOptions.Count(option => option.IsCorrect) != 1)
                {
                    Add(issues, "PracticeItemCorrectOptionMissing", "Every practice item must include exactly one correct answer option.");
                }
            }
        }

        var materials = version.GetOrderedMaterials();
        if (materials.Count == 0)
        {
            Add(issues, "PackageMaterialsMissing", "Package version must include at least one material version.");
        }

        if (materials.Any(m => m.SortOrder < 1) || materials.Select(m => m.SortOrder).Distinct().Count() != materials.Count)
        {
            Add(issues, "PackageMaterialSortOrderInvalid", "Material sort orders must be positive and unique.");
        }

        foreach (var material in materials)
        {
            var materialVersion = await _context.StudyMaterialVersions.FirstOrDefaultAsync(v => v.Id == material.StudyMaterialVersionId, cancellationToken);
            if (materialVersion is null || materialVersion.Status != PublicationStatus.Published)
            {
                Add(issues, "MaterialVersionNotPublished", "Every referenced material version must be published.");
                continue;
            }

            if (!materialVersion.Topics.Any())
            {
                Add(issues, "MaterialTopicMappingMissing", "Every material version must map to at least one reporting topic.");
            }

            foreach (var topicId in materialVersion.Topics.Select(t => t.ReportingTopicId))
            {
                var topic = await _context.ReportingTopics.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken);
                if (topic is null || topic.ExamCategoryId != definition.ExamCategoryId || !topic.IsActive)
                {
                    Add(issues, "MaterialTopicContextMismatch", "Material reporting topics must belong to the package exam category context.");
                }
            }
        }

        if (!version.ContentIsolationConfirmed)
        {
            Add(issues, "ContentIsolationMissing", "Package version content isolation must be confirmed before publication.");
        }

        return ToDto(issues);
    }

    public async Task ValidateReportingProfilePublicationAsync(
        ReportingProfilePublication profile,
        IReadOnlyCollection<(Guid ExamQuestionId, Guid ReportingTopicId)> assignments,
        CancellationToken cancellationToken)
    {
        if (assignments.Count == 0)
        {
            throw new InvalidOperationException("A reporting profile publication must include question assignments.");
        }

        var examVersion = await _context.ExamVersions.FirstOrDefaultAsync(v => v.Id == profile.ExamVersionId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam version was not found.");
        if (examVersion.Status != ExamVersionStatus.Published)
        {
            throw new InvalidOperationException("Reporting profile publication requires a published exam version.");
        }

        var publishedProfileExists = await _context.ReportingProfilePublications.AnyAsync(
            p => p.Id != profile.Id
                && p.ExamVersionId == profile.ExamVersionId
                && p.Status == PublicationStatus.Published,
            cancellationToken);
        if (publishedProfileExists)
        {
            throw new InvalidOperationException("A published reporting profile already exists for this exam version.");
        }

        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examVersion.ExamId, cancellationToken)
            ?? throw new KeyNotFoundException("Exam was not found.");
        var questionIds = await _context.ExamQuestions
            .Where(q => q.ExamVersionId == profile.ExamVersionId && q.IsActive && q.Points > 0)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        if (questionIds.Count == 0 || questionIds.Except(assignments.Select(a => a.ExamQuestionId)).Any())
        {
            throw new InvalidOperationException("Reporting profile must assign every active scored question in the exam version.");
        }

        foreach (var assignment in assignments)
        {
            if (!questionIds.Contains(assignment.ExamQuestionId))
            {
                throw new InvalidOperationException("Reporting profile assignments must reference questions in the exact exam version.");
            }

            var topic = await _context.ReportingTopics.FirstOrDefaultAsync(t => t.Id == assignment.ReportingTopicId, cancellationToken)
                ?? throw new InvalidOperationException("Reporting profile assignments must reference existing reporting topics.");
            if (topic.ExamCategoryId != exam.ExamCategoryId || !topic.IsActive)
            {
                throw new InvalidOperationException("Reporting profile topics must match the exam version category context.");
            }
        }
    }

    private static PackagePublicationValidationDto ToDto(List<PackagePublicationValidationIssueDto> issues)
    {
        return new PackagePublicationValidationDto { Issues = issues };
    }

    private static void Add(List<PackagePublicationValidationIssueDto> issues, string code, string message)
    {
        issues.Add(new PackagePublicationValidationIssueDto { Code = code, Message = message });
    }
}
