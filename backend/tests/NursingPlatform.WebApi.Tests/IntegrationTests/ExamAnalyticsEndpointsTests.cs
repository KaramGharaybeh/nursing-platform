using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ExamAnalyticsEndpointsTests
{
    private static readonly string[] ForbiddenPatterns =
    [
        "\"passwordHash\"",
        "\"roles\"",
        "\"permissions\"",
        "\"accessToken\"",
        "\"refreshToken\"",
        "\"tokenHash\"",
        "\"paymentProviderId\"",
        "\"paymentIntentId\"",
        "\"orderId\"",
        "\"user\"",
        "\"nurseProfile\"",
        "\"examSession\"",
        "\"examSessionAnswer\"",
        "\"examSessionQuestion\"",
        "\"examSessionAnswerOption\"",
        "\"selectedExamSessionAnswerOptionId\"",
        "\"correctAnswerOptionId\"",
        "\"isCorrect\"",
        "\"explanation\""
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ExamAnalyticsEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/me/nurse-profile/exam-analytics/summary")]
    [InlineData("/api/v1/me/nurse-profile/exam-analytics/by-exam")]
    [InlineData("/api/v1/me/nurse-profile/exam-analytics/by-category")]
    [InlineData("/api/v1/me/nurse-profile/exam-analytics/trends")]
    public async Task ExamAnalyticsEndpoints_WithoutJwt_ReturnUnauthorized(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExamAnalyticsEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyExamAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamAnalyticsSummaryDto());

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-analytics/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_WithFilters_SendsQuery()
    {
        var countryId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<GetMyExamAnalyticsSummaryQuery>(q =>
                q.CountryId == countryId && q.CategoryId == categoryId && q.ExamId == examId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamAnalyticsSummaryDto());

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/exam-analytics/summary?countryId={countryId}&categoryId={categoryId}&examId={examId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListByExam_WithPaginationAndFilters_SendsQuery()
    {
        var countryId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyExamAnalyticsByExamQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.CountryId == countryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ExamAnalyticsByExamDto> { Page = 2, PageSize = 5 });

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/exam-analytics/by-exam?page=2&pageSize=5&countryId={countryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListByCategory_WithPaginationAndFilters_SendsQuery()
    {
        var categoryId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyExamAnalyticsByCategoryQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.CategoryId == categoryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ExamAnalyticsByCategoryDto> { Page = 2, PageSize = 5 });

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/exam-analytics/by-category?page=2&pageSize=5&categoryId={categoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListTrends_WithBucketAndFilters_SendsQuery()
    {
        var examId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyExamAnalyticsTrendsQuery>(q =>
                q.Bucket == ExamAnalyticsBucket.Week && q.ExamId == examId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/exam-analytics/trends?bucket=week&examId={examId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExamAnalyticsEndpoints_WithInvalidGuidFilter_ReturnBadRequestAndSenderNotCalled()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-analytics/summary?countryId=not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExamAnalyticsValidationFailure_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyExamAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("To", "To must be after From.")]));

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-analytics/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExamAnalyticsForbiddenAccess_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyExamAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Nurse role is required."));

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-analytics/summary");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ExamAnalyticsJson_DoesNotExposeForbiddenFields()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyExamAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamAnalyticsSummaryDto
            {
                AttemptCount = 4,
                CountedAttemptCount = 2,
                InProgressCount = 1,
                AverageScorePercentage = 80
            });

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-analytics/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPatterns);
        var body = JsonSerializer.Deserialize<ExamAnalyticsSummaryDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(1, body.InProgressCount);
    }

    private static void AssertDoesNotContain(string json, IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
