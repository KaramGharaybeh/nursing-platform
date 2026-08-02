using System.Reflection;
using FluentValidation.TestHelper;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Application.PreparationPackages.PracticeProgress;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

namespace NursingPlatform.Application.Tests.PreparationPackages.PracticeProgress;

public class PackagePracticeProgressContractTests
{
    [Fact]
    public void GetPackagePracticeProgressQuery_ShouldUsePackageEntitlementOnly()
    {
        AssertProperties(typeof(GetPackagePracticeProgressQuery), "EntitlementId");
        Assert.Equal(typeof(Guid), typeof(GetPackagePracticeProgressQuery).GetProperty("EntitlementId")!.PropertyType);
    }

    [Fact]
    public void SubmitPackagePracticeAnswerCommand_ShouldUsePackagePracticeIdentifiersOnly()
    {
        AssertProperties(typeof(SubmitPackagePracticeAnswerCommand), "EntitlementId", "PracticeItemId", "Request");
        Assert.Equal(typeof(Guid), typeof(SubmitPackagePracticeAnswerCommand).GetProperty("EntitlementId")!.PropertyType);
        Assert.Equal(typeof(Guid), typeof(SubmitPackagePracticeAnswerCommand).GetProperty("PracticeItemId")!.PropertyType);
        Assert.Equal(typeof(SubmitPackagePracticeAnswerRequest), typeof(SubmitPackagePracticeAnswerCommand).GetProperty("Request")!.PropertyType);
    }

    [Fact]
    public void SubmitPackagePracticeAnswerRequest_ShouldAcceptSelectedPracticeAnswerOptionOnly()
    {
        AssertProperties(typeof(SubmitPackagePracticeAnswerRequest), "SelectedPracticeAnswerOptionId");
        Assert.Equal(typeof(Guid), typeof(SubmitPackagePracticeAnswerRequest).GetProperty("SelectedPracticeAnswerOptionId")!.PropertyType);
    }

    [Fact]
    public void PackagePracticeProgressSummaryDto_ShouldExposeDerivedCountersAndItemStates()
    {
        AssertProperties(
            typeof(PackagePracticeProgressSummaryDto),
            "PackagePurchaseEntitlementId",
            "PracticeCollectionVersionId",
            "TotalItems",
            "AnsweredCount",
            "UnansweredCount",
            "CorrectCount",
            "IncorrectCount",
            "ItemStates");
    }

    [Fact]
    public void PackagePracticeProgressItemStateDto_ShouldExposePracticeItemStateOnly()
    {
        AssertProperties(
            typeof(PackagePracticeProgressItemStateDto),
            "PracticeItemId",
            "State",
            "SelectedPracticeAnswerOptionId",
            "LastAnsweredAt");

        Assert.Equal(typeof(Guid), typeof(PackagePracticeProgressItemStateDto).GetProperty("PracticeItemId")!.PropertyType);
        Assert.Equal(typeof(PackagePracticeProgressItemState), typeof(PackagePracticeProgressItemStateDto).GetProperty("State")!.PropertyType);
        Assert.Equal(typeof(Guid?), typeof(PackagePracticeProgressItemStateDto).GetProperty("SelectedPracticeAnswerOptionId")!.PropertyType);
        Assert.Equal(typeof(DateTime?), typeof(PackagePracticeProgressItemStateDto).GetProperty("LastAnsweredAt")!.PropertyType);
    }

    [Fact]
    public void PackagePracticeAnswerSubmissionDto_ShouldExposeLatestSubmittedPracticeAnswerState()
    {
        AssertProperties(
            typeof(PackagePracticeAnswerSubmissionDto),
            "PracticeItemId",
            "State",
            "SelectedPracticeAnswerOptionId",
            "LastAnsweredAt");

        Assert.Equal(typeof(Guid), typeof(PackagePracticeAnswerSubmissionDto).GetProperty("PracticeItemId")!.PropertyType);
        Assert.Equal(typeof(PackagePracticeProgressItemState), typeof(PackagePracticeAnswerSubmissionDto).GetProperty("State")!.PropertyType);
        Assert.Equal(typeof(Guid), typeof(PackagePracticeAnswerSubmissionDto).GetProperty("SelectedPracticeAnswerOptionId")!.PropertyType);
        Assert.Equal(typeof(DateTime), typeof(PackagePracticeAnswerSubmissionDto).GetProperty("LastAnsweredAt")!.PropertyType);
    }

    [Fact]
    public void PackagePracticeProgressItemState_ShouldIncludeDerivedUnansweredAndAnsweredStates()
    {
        Assert.Equal(0, (int)PackagePracticeProgressItemState.Unanswered);
        Assert.Equal(1, (int)PackagePracticeProgressItemState.AnsweredCorrect);
        Assert.Equal(2, (int)PackagePracticeProgressItemState.AnsweredIncorrect);
    }

    [Fact]
    public void GetPackagePracticeProgressQueryValidator_ShouldRejectEmptyEntitlementId()
    {
        new GetPackagePracticeProgressQueryValidator()
            .TestValidate(new GetPackagePracticeProgressQuery(Guid.Empty))
            .ShouldHaveValidationErrorFor(query => query.EntitlementId);
    }

    [Fact]
    public void SubmitPackagePracticeAnswerCommandValidator_ShouldRejectEmptyRouteAndRequestIdentifiers()
    {
        var result = new SubmitPackagePracticeAnswerCommandValidator()
            .TestValidate(new SubmitPackagePracticeAnswerCommand(
                Guid.Empty,
                Guid.Empty,
                new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = Guid.Empty }));

        result.ShouldHaveValidationErrorFor(command => command.EntitlementId);
        result.ShouldHaveValidationErrorFor(command => command.PracticeItemId);
        result.ShouldHaveValidationErrorFor("Request.SelectedPracticeAnswerOptionId");
    }

    [Fact]
    public void SubmitPackagePracticeAnswerCommandValidator_ShouldRejectMissingRequest()
    {
        new SubmitPackagePracticeAnswerCommandValidator()
            .TestValidate(new SubmitPackagePracticeAnswerCommand(Guid.NewGuid(), Guid.NewGuid(), null!))
            .ShouldHaveValidationErrorFor(command => command.Request);
    }

    [Fact]
    public void SubmitPackagePracticeAnswerRequestValidator_ShouldRejectEmptySelectedPracticeAnswerOptionId()
    {
        new SubmitPackagePracticeAnswerRequestValidator()
            .TestValidate(new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = Guid.Empty })
            .ShouldHaveValidationErrorFor(request => request.SelectedPracticeAnswerOptionId);
    }

    [Fact]
    public void PracticeProgressContracts_ShouldNotExposeOfficialExamOrInternalRightFields()
    {
        var contractTypes = new[]
        {
            typeof(GetPackagePracticeProgressQuery),
            typeof(SubmitPackagePracticeAnswerCommand),
            typeof(SubmitPackagePracticeAnswerRequest),
            typeof(PackagePracticeProgressSummaryDto),
            typeof(PackagePracticeProgressItemStateDto),
            typeof(PackagePracticeAnswerSubmissionDto)
        };

        var forbiddenTerms = new[]
        {
            "Exam" + "Question" + "Id",
            "Exam" + "Answer" + "Option" + "Id",
            "Exam" + "Session" + "Id",
            "Exam" + "Session",
            "Question" + "Text" + "Snapshot",
            "Option" + "Text" + "Snapshot",
            "Correct" + "Answer",
            "Correct" + "Option",
            "Answer" + "Key",
            "Ration" + "ale",
            "Explanation" + "Snapshot",
            "Benefit" + "Right" + "Id",
            "Package" + "Benefit" + "Right" + "Id"
        };

        Assert.All(contractTypes.SelectMany(GetPublicPropertyNames), propertyName =>
        {
            Assert.DoesNotContain(forbiddenTerms, term =>
                propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void PracticeProgressTypes_ShouldBeDiscoverableFromApplicationAssembly()
    {
        var assembly = typeof(PreparationPackageOfferListItemDto).Assembly;

        Assert.NotNull(assembly.GetType("NursingPlatform.Application.PreparationPackages.PracticeProgress.GetPackagePracticeProgressQuery"));
        Assert.NotNull(assembly.GetType("NursingPlatform.Application.PreparationPackages.PracticeProgress.SubmitPackagePracticeAnswerCommand"));
        Assert.NotNull(assembly.GetType("NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeProgressSummaryDto"));
        Assert.NotNull(assembly.GetType("NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeProgressItemStateDto"));
        Assert.NotNull(assembly.GetType("NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeAnswerSubmissionDto"));
    }

    private static IEnumerable<string> GetPublicPropertyNames(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name);
    }

    private static void AssertProperties(Type type, params string[] expectedPropertyNames)
    {
        var actualNames = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order()
            .ToArray();

        Assert.Equal(expectedPropertyNames.Order().ToArray(), actualNames);
    }
}
