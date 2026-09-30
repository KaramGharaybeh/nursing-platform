using System.Reflection;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.Application.Tests.PreparationPackages;

public class PreparationPackageDtoSecurityTests
{
    private const string ReportsNamespace = "NursingPlatform.Application.PreparationPackages.Reports.DTOs";

    private static readonly string[] CatalogForbiddenTerms =
    [
        "questiontext",
        "answerkey",
        "answeroption",
        "protectedanswer",
        "iscorrect",
        "correctanswer",
        "rationale",
        "explanation",
        "scoring",
        "reportlogic",
        "authorization",
        "permission",
        "token",
        "passwordhash",
        "paymentprovider",
        "clientsecret"
    ];

    private static readonly string[] AdminForbiddenTerms =
    [
        "passwordhash",
        "token",
        "paymentprovider",
        "clientsecret",
        "navigation",
        "entity"
    ];

    [Fact]
    public void CatalogDtos_ShouldNotExposeProtectedExamContentOrInternalAuthorizationState()
    {
        var catalogDtoTypes = new[]
        {
            typeof(PreparationPackageOfferListItemDto),
            typeof(PreparationPackageOfferDetailDto),
            typeof(PreparationPackageCatalogComponentSummaryDto)
        };

        Assert.All(catalogDtoTypes.SelectMany(GetPublicPropertyNames), propertyName =>
        {
            Assert.DoesNotContain(CatalogForbiddenTerms, term =>
                propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void AdminDtos_ShouldNotExposePasswordHashesTokensPaymentProviderIdsOrNavigationObjects()
    {
        var adminDtoTypes = new[]
        {
            typeof(AdminReportingTopicDto),
            typeof(AdminReportingProfilePublicationDto),
            typeof(AdminStudyMaterialDto),
            typeof(AdminStudyMaterialVersionDto),
            typeof(AdminPracticeCollectionDto),
            typeof(AdminPracticeCollectionVersionDto),
            typeof(AdminPracticeItemDto),
            typeof(AdminPracticeAnswerOptionDto),
            typeof(AdminPreparationPackageDefinitionDto),
            typeof(AdminPreparationPackageVersionDto),
            typeof(AdminPreparationPackageOfferDto),
            typeof(PackagePublicationValidationDto),
            typeof(PackagePublicationValidationIssueDto)
        };

        Assert.All(adminDtoTypes.SelectMany(GetPublicPropertyNames), propertyName =>
        {
            Assert.DoesNotContain(AdminForbiddenTerms, term =>
                propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void PackageAnalyticalReportDtos_ShouldNotExposeProtectedContentInternalRightsOrPaymentInternals()
    {
        var reportDtoTypes = new[]
        {
            RequireApplicationType($"{ReportsNamespace}.PackageAnalyticalReportDto"),
            RequireApplicationType($"{ReportsNamespace}.PackageAnalyticalReportTopicResultDto"),
            RequireApplicationType($"{ReportsNamespace}.PackageAnalyticalReportGuidanceItemDto")
        };

        var forbiddenTerms = new[]
        {
            "packagebenefitrightid",
            "benefitrightid",
            "paymentorderid",
            "paymentorderitemid",
            "paymentprovider",
            "providersecret",
            "clientsecret",
            "secret",
            "questiontext",
            "optiontext",
            "answeroption",
            "correctanswer",
            "iscorrect",
            "answerkey",
            "rationale",
            "explanation",
            "protectedanswer",
            "navigation",
            "entity",
            "authorization",
            "permission",
            "token",
            "passwordhash",
            "stacktrace"
        };

        Assert.All(reportDtoTypes.SelectMany(GetPublicPropertyNames), propertyName =>
        {
            Assert.DoesNotContain(forbiddenTerms, term =>
                propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void PracticeItemAuthoringContractsAndDtos_ShouldNotExposeOfficialExamIdentifiersOrSnapshots()
    {
        var practiceContractTypes = new[]
        {
            typeof(CreateAdminPracticeCollectionVersionRequest),
            typeof(UpdateAdminPracticeCollectionVersionRequest),
            typeof(UpsertAdminPracticeItemRequest),
            typeof(UpsertAdminPracticeAnswerOptionRequest),
            typeof(AdminPracticeCollectionVersionDto),
            typeof(AdminPracticeItemDto),
            typeof(AdminPracticeAnswerOptionDto)
        };

        var forbiddenTerms = new[]
        {
            "ExamQuestionId",
            "ExamAnswerOptionId",
            "ExamSessionId",
            "QuestionTextSnapshot",
            "OptionTextSnapshot",
            "ExplanationSnapshot",
            "AnswerKey",
            "Rationale",
            "CorrectAnswer",
            "CorrectOption"
        };

        Assert.All(practiceContractTypes.SelectMany(GetPublicPropertyNames), propertyName =>
        {
            Assert.DoesNotContain(forbiddenTerms, term =>
                propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static IEnumerable<string> GetPublicPropertyNames(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name);
    }

    private static Type RequireApplicationType(string fullName)
    {
        return typeof(PreparationPackageOfferListItemDto).Assembly.GetType(fullName)
            ?? throw new InvalidOperationException($"Expected Application type '{fullName}' to exist.");
    }
}
