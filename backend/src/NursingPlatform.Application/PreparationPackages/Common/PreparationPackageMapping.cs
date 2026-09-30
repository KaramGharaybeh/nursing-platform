using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Common;

internal static class PreparationPackageMapping
{
    public static AdminReportingTopicDto ToReportingTopicDto(ReportingTopic topic, string categoryName)
    {
        return new AdminReportingTopicDto
        {
            Id = topic.Id,
            ExamCategoryId = topic.ExamCategoryId,
            ExamCategoryName = categoryName,
            Name = topic.Name,
            Slug = topic.Slug,
            IsActive = topic.IsActive
        };
    }

    public static AdminReportingProfilePublicationDto ToReportingProfileDto(
        ReportingProfilePublication profile,
        IEnumerable<ReportingProfileQuestionAssignment> assignments,
        IReadOnlyDictionary<Guid, ReportingTopic> topicsById)
    {
        return new AdminReportingProfilePublicationDto
        {
            Id = profile.Id,
            ExamVersionId = profile.ExamVersionId,
            Name = profile.Name,
            Status = profile.Status.ToString(),
            PublishedAt = profile.PublishedAt,
            Assignments = assignments
                .OrderBy(a => a.ExamQuestionId)
                .Select(a => new AdminReportingProfileQuestionAssignmentDto
                {
                    ExamQuestionId = a.ExamQuestionId,
                    ReportingTopicId = a.ReportingTopicId,
                    ReportingTopicName = topicsById.TryGetValue(a.ReportingTopicId, out var topic) ? topic.Name : string.Empty
                })
                .ToList()
        };
    }

    public static AdminStudyMaterialDto ToStudyMaterialDto(StudyMaterial material)
    {
        return new AdminStudyMaterialDto
        {
            Id = material.Id,
            Title = material.Title,
            Slug = material.Slug,
            Description = material.Description
        };
    }

    public static AdminStudyMaterialVersionDto ToStudyMaterialVersionDto(StudyMaterialVersion version)
    {
        return new AdminStudyMaterialVersionDto
        {
            Id = version.Id,
            StudyMaterialId = version.StudyMaterialId,
            VersionNumber = version.VersionNumber,
            MaterialType = version.MaterialType.ToString(),
            Status = version.Status.ToString(),
            FormattedTextContent = version.FormattedTextContent,
            FileStorageKey = version.FileStorageKey,
            ExternalUrl = version.ExternalUrl,
            VideoUrl = version.VideoUrl,
            PublishedAt = version.PublishedAt,
            ReportingTopicIds = version.Topics.OrderBy(t => t.ReportingTopicId).Select(t => t.ReportingTopicId).ToList()
        };
    }

    public static AdminPracticeCollectionDto ToPracticeCollectionDto(PracticeCollection collection)
    {
        return new AdminPracticeCollectionDto
        {
            Id = collection.Id,
            Title = collection.Title,
            Slug = collection.Slug,
            Description = collection.Description
        };
    }

    public static AdminPracticeCollectionVersionDto ToPracticeCollectionVersionDto(PracticeCollectionVersion version)
    {
        return new AdminPracticeCollectionVersionDto
        {
            Id = version.Id,
            PracticeCollectionId = version.PracticeCollectionId,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            PublishedAt = version.PublishedAt,
            Items = version.Items
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.Id)
                .Select(i => new AdminPracticeItemDto
                {
                    Id = i.Id,
                    ReportingTopicId = i.ReportingTopicId,
                    Prompt = i.Prompt,
                    ImmediateFeedback = i.ImmediateFeedback,
                    DisplayOrder = i.DisplayOrder,
                    Options = i.AnswerOptions
                        .OrderBy(o => o.DisplayOrder)
                        .ThenBy(o => o.Id)
                        .Select(o => new AdminPracticeAnswerOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            DisplayOrder = o.DisplayOrder,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public static AdminPreparationPackageDefinitionDto ToPackageDefinitionDto(
        PreparationPackageDefinition definition,
        string countryName,
        string categoryName)
    {
        return new AdminPreparationPackageDefinitionDto
        {
            Id = definition.Id,
            CountryId = definition.CountryId,
            CountryName = countryName,
            ExamCategoryId = definition.ExamCategoryId,
            ExamCategoryName = categoryName,
            Title = definition.Title,
            Slug = definition.Slug,
            Description = definition.Description
        };
    }

    public static AdminPreparationPackageVersionDto ToPackageVersionDto(PreparationPackageVersion version)
    {
        return new AdminPreparationPackageVersionDto
        {
            Id = version.Id,
            PreparationPackageDefinitionId = version.PreparationPackageDefinitionId,
            ExamVersionId = version.ExamVersionId,
            ReportingProfilePublicationId = version.ReportingProfilePublicationId,
            PracticeCollectionVersionId = version.PracticeCollectionVersionId,
            Status = version.Status.ToString(),
            ContentIsolationConfirmed = version.ContentIsolationConfirmed,
            PublishedAt = version.PublishedAt,
            Materials = version.GetOrderedMaterials()
                .Select(m => new PreparationPackageVersionMaterialDto
                {
                    StudyMaterialVersionId = m.StudyMaterialVersionId,
                    SortOrder = m.SortOrder
                })
                .ToList()
        };
    }

    public static AdminPreparationPackageOfferDto ToPackageOfferDto(PreparationPackageOffer offer)
    {
        return new AdminPreparationPackageOfferDto
        {
            Id = offer.Id,
            PreparationPackageDefinitionId = offer.PreparationPackageDefinitionId,
            PreparationPackageVersionId = offer.PreparationPackageVersionId,
            Title = offer.Title,
            Slug = offer.Slug,
            Summary = offer.Summary,
            PriceAmountMinor = offer.PriceAmountMinor,
            Currency = offer.Currency,
            AccessDurationDays = offer.AccessDurationDays,
            Status = offer.Status.ToString()
        };
    }
}
