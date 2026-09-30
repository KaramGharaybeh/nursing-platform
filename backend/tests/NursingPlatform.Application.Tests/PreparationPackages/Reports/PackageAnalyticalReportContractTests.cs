using FluentValidation;
using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.Application.Tests.PreparationPackages.Reports;

public class PackageAnalyticalReportContractTests
{
    private const string ReportsNamespace = "NursingPlatform.Application.PreparationPackages.Reports";
    private const string DtosNamespace = $"{ReportsNamespace}.DTOs";
    private const string QueryNamespace = $"{ReportsNamespace}.GetPackageAnalyticalReport";

    [Fact]
    public void PackageAnalyticalReportDto_ShouldExposeOnlySafePublicReportShape()
    {
        var dtoType = RequireApplicationType($"{DtosNamespace}.PackageAnalyticalReportDto");

        AssertProperties(dtoType,
            "Id",
            "ExamSessionId",
            "GeneratedAt",
            "FinalizedSessionStatus",
            "SubmittedAt",
            "FinalizedAt",
            "Score",
            "MaxScore",
            "Percentage",
            "Passed",
            "CorrectCount",
            "QuestionCount",
            "TopicResults",
            "GuidanceItems");
    }

    [Fact]
    public void PackageAnalyticalReportTopicResultDto_ShouldExposeCountsPercentagesAndNoBandsOrLabels()
    {
        var dtoType = RequireApplicationType($"{DtosNamespace}.PackageAnalyticalReportTopicResultDto");

        AssertProperties(dtoType,
            "ReportingTopicId",
            "TopicName",
            "TopicDescription",
            "ScoredQuestionCount",
            "CorrectCount",
            "EarnedPoints",
            "AvailablePoints",
            "Percentage",
            "SortOrder");

        Assert.DoesNotContain(dtoType.GetProperties(), property =>
            property.Name.Contains("Band", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Label", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Strength", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("Weak", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PackageAnalyticalReportGuidanceItemDto_ShouldExposeOnlySafePurchasedContentReferences()
    {
        var dtoType = RequireApplicationType($"{DtosNamespace}.PackageAnalyticalReportGuidanceItemDto");

        AssertProperties(dtoType,
            "ReportingTopicId",
            "SourceType",
            "SourceVersionId",
            "Title",
            "SourceMetadata",
            "SortOrder");
    }

    [Fact]
    public void GetPackageAnalyticalReportQuery_ShouldRequireDirectExamSessionIdAccess()
    {
        var queryType = RequireApplicationType($"{QueryNamespace}.GetPackageAnalyticalReportQuery");

        var sessionIdProperty = queryType.GetProperty("SessionId");

        Assert.NotNull(sessionIdProperty);
        Assert.Equal(typeof(Guid), sessionIdProperty.PropertyType);
        Assert.DoesNotContain(queryType.GetProperties(), property =>
            property.Name.Contains("ReportId", StringComparison.OrdinalIgnoreCase)
            || property.Name.Contains("PackageBenefitRightId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetPackageAnalyticalReportQueryValidator_ShouldRejectEmptySessionId()
    {
        var queryType = RequireApplicationType($"{QueryNamespace}.GetPackageAnalyticalReportQuery");
        var validatorType = RequireApplicationType($"{QueryNamespace}.GetPackageAnalyticalReportQueryValidator");
        var validator = Assert.IsAssignableFrom<IValidator>(Activator.CreateInstance(validatorType));
        var query = Activator.CreateInstance(queryType, Guid.Empty) ?? Activator.CreateInstance(queryType)!;

        var result = validator.Validate(new ValidationContext<object>(query));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == "SessionId");
    }

    private static Type RequireApplicationType(string fullName)
    {
        return typeof(PreparationPackageOfferListItemDto).Assembly.GetType(fullName)
            ?? throw new InvalidOperationException($"Expected Application type '{fullName}' to exist.");
    }

    private static void AssertProperties(Type type, params string[] expectedPropertyNames)
    {
        var actualNames = type.GetProperties().Select(property => property.Name).Order().ToArray();

        Assert.Equal(expectedPropertyNames.Order().ToArray(), actualNames);
    }
}
