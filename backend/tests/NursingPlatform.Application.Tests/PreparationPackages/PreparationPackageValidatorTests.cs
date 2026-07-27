using FluentValidation.TestHelper;
using NursingPlatform.Application.PreparationPackages.Admin.PackageDefinitions;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.Catalog;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.Tests.PreparationPackages;

public class PreparationPackageValidatorTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_Pagination_WithInvalidPageOrPageSize_ShouldHaveError(int page, int pageSize)
    {
        var catalogResult = new ListPreparationPackageOffersQueryValidator()
            .TestValidate(new ListPreparationPackageOffersQuery { Page = page, PageSize = pageSize });
        var adminResult = new ListAdminPreparationPackageDefinitionsQueryValidator()
            .TestValidate(new ListAdminPreparationPackageDefinitionsQuery { Page = page, PageSize = pageSize });

        if (page < 1)
        {
            catalogResult.ShouldHaveValidationErrorFor(x => x.Page);
            adminResult.ShouldHaveValidationErrorFor(x => x.Page);
        }

        if (pageSize is < 1 or > 100)
        {
            catalogResult.ShouldHaveValidationErrorFor(x => x.PageSize);
            adminResult.ShouldHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [Fact]
    public void Validate_CreateReportingTopic_WithEmptyNameOrCategoryId_ShouldHaveError()
    {
        var result = new CreateAdminReportingTopicCommandValidator()
            .TestValidate(new CreateAdminReportingTopicCommand
            {
                Request = new CreateAdminReportingTopicRequest
                {
                    ExamCategoryId = Guid.Empty,
                    Name = "",
                    Slug = ""
                }
            });

        result.ShouldHaveValidationErrorFor("Request.ExamCategoryId");
        result.ShouldHaveValidationErrorFor("Request.Name");
        result.ShouldHaveValidationErrorFor("Request.Slug");
    }

    [Fact]
    public void Validate_PublishReportingProfile_WithMissingAssignments_ShouldHaveError()
    {
        new PublishAdminReportingProfileCommandValidator()
            .TestValidate(new PublishAdminReportingProfileCommand
            {
                Id = Guid.NewGuid(),
                Request = new PublishAdminReportingProfileRequest { Assignments = [] }
            })
            .ShouldHaveValidationErrorFor("Request.Assignments");
    }

    [Fact]
    public void Validate_MaterialVersion_WithUnsupportedMaterialTypeFields_ShouldHaveError()
    {
        var result = new CreateAdminStudyMaterialVersionCommandValidator()
            .TestValidate(new CreateAdminStudyMaterialVersionCommand
            {
                StudyMaterialId = Guid.NewGuid(),
                Request = new CreateAdminStudyMaterialVersionRequest
                {
                    MaterialType = StudyMaterialType.File,
                    FileStorageKey = "files/nclex.pdf",
                    ExternalUrl = "https://example.com/should-not-be-present",
                    ReportingTopicIds = [Guid.NewGuid()]
                }
            });

        result.ShouldHaveValidationErrorFor("Request.ExternalUrl");
    }

    [Fact]
    public void Validate_MaterialVersion_WithNoTopicMappings_ShouldHaveError()
    {
        new CreateAdminStudyMaterialVersionCommandValidator()
            .TestValidate(new CreateAdminStudyMaterialVersionCommand
            {
                StudyMaterialId = Guid.NewGuid(),
                Request = new CreateAdminStudyMaterialVersionRequest
                {
                    MaterialType = StudyMaterialType.FormattedText,
                    FormattedTextContent = "Study this topic.",
                    ReportingTopicIds = []
                }
            })
            .ShouldHaveValidationErrorFor("Request.ReportingTopicIds");
    }

    [Fact]
    public void Validate_PracticeItem_WithNoTopicOrNoCorrectAnswer_ShouldHaveError()
    {
        var result = new UpsertAdminPracticeItemRequestValidator()
            .TestValidate(new UpsertAdminPracticeItemRequest
            {
                ReportingTopicId = Guid.Empty,
                Prompt = "Practice prompt",
                ImmediateFeedback = "Practice feedback",
                DisplayOrder = 1,
                AnswerOptions =
                [
                    new UpsertAdminPracticeAnswerOptionRequest { OptionText = "A", DisplayOrder = 1, IsCorrect = false },
                    new UpsertAdminPracticeAnswerOptionRequest { OptionText = "B", DisplayOrder = 2, IsCorrect = false }
                ]
            });

        result.ShouldHaveValidationErrorFor(x => x.ReportingTopicId);
        result.ShouldHaveValidationErrorFor(x => x.AnswerOptions);
    }

    [Fact]
    public void Validate_PackageVersion_WithDuplicateMaterialSortOrder_ShouldHaveError()
    {
        new CreateAdminPreparationPackageVersionCommandValidator()
            .TestValidate(new CreateAdminPreparationPackageVersionCommand
            {
                PreparationPackageDefinitionId = Guid.NewGuid(),
                Request = new CreateAdminPreparationPackageVersionRequest
                {
                    ExamVersionId = Guid.NewGuid(),
                    ReportingProfilePublicationId = Guid.NewGuid(),
                    PracticeCollectionVersionId = Guid.NewGuid(),
                    Materials =
                    [
                        new PreparationPackageVersionMaterialRequest { StudyMaterialVersionId = Guid.NewGuid(), SortOrder = 1 },
                        new PreparationPackageVersionMaterialRequest { StudyMaterialVersionId = Guid.NewGuid(), SortOrder = 1 }
                    ]
                }
            })
            .ShouldHaveValidationErrorFor("Request.Materials");
    }

    [Fact]
    public void Validate_PackageOffer_WithInvalidPriceCurrencyOrDuration_ShouldHaveError()
    {
        var result = new CreateAdminPreparationPackageOfferCommandValidator()
            .TestValidate(new CreateAdminPreparationPackageOfferCommand
            {
                Request = new CreateAdminPreparationPackageOfferRequest
                {
                    PreparationPackageDefinitionId = Guid.NewGuid(),
                    PreparationPackageVersionId = Guid.NewGuid(),
                    Title = "Offer",
                    Slug = "offer",
                    PriceAmountMinor = -1,
                    Currency = "usd",
                    AccessDurationDays = 0
                }
            });

        result.ShouldHaveValidationErrorFor("Request.PriceAmountMinor");
        result.ShouldHaveValidationErrorFor("Request.Currency");
        result.ShouldHaveValidationErrorFor("Request.AccessDurationDays");
    }
}
