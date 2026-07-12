using FluentValidation.TestHelper;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;

namespace NursingPlatform.Application.Tests.Exams.Analytics;

public class ExamAnalyticsValidatorTests
{
    [Fact]
    public void Validate_Summary_WithFromAfterTo_ShouldHaveError()
    {
        var result = new GetMyExamAnalyticsSummaryQueryValidator()
            .TestValidate(new GetMyExamAnalyticsSummaryQuery
            {
                From = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ByExam_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var result = new ListMyExamAnalyticsByExamQueryValidator()
            .TestValidate(new ListMyExamAnalyticsByExamQuery { Page = page, PageSize = pageSize });

        if (page < 1)
        {
            result.ShouldHaveValidationErrorFor(x => x.Page);
        }

        if (pageSize is < 1 or > 100)
        {
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ByCategory_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var result = new ListMyExamAnalyticsByCategoryQueryValidator()
            .TestValidate(new ListMyExamAnalyticsByCategoryQuery { Page = page, PageSize = pageSize });

        if (page < 1)
        {
            result.ShouldHaveValidationErrorFor(x => x.Page);
        }

        if (pageSize is < 1 or > 100)
        {
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [Fact]
    public void Validate_Trends_WithInvalidBucket_ShouldHaveError()
    {
        var result = new ListMyExamAnalyticsTrendsQueryValidator()
            .TestValidate(new ListMyExamAnalyticsTrendsQuery { Bucket = (ExamAnalyticsBucket)99 });

        result.ShouldHaveValidationErrorFor(x => x.Bucket);
    }

    [Fact]
    public void Validate_Trends_WithTooLargeDailyRange_ShouldHaveError()
    {
        var result = new ListMyExamAnalyticsTrendsQueryValidator()
            .TestValidate(new ListMyExamAnalyticsTrendsQuery
            {
                Bucket = ExamAnalyticsBucket.Day,
                From = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                To = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            });

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public void Validate_Filters_WithEmptyGuidValues_ShouldHaveError()
    {
        var summary = new GetMyExamAnalyticsSummaryQueryValidator()
            .TestValidate(new GetMyExamAnalyticsSummaryQuery
            {
                CountryId = Guid.Empty,
                CategoryId = Guid.Empty,
                ExamId = Guid.Empty
            });

        summary.ShouldHaveValidationErrorFor(x => x.CountryId);
        summary.ShouldHaveValidationErrorFor(x => x.CategoryId);
        summary.ShouldHaveValidationErrorFor(x => x.ExamId);
    }
}
