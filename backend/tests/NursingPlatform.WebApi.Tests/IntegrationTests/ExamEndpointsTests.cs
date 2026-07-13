using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Commands.SubmitExamSession;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Exams.Queries.GetExam;
using NursingPlatform.Application.Exams.Queries.GetExamSession;
using NursingPlatform.Application.Exams.Queries.GetExamSessionResult;
using NursingPlatform.Application.Exams.Queries.GetExamSessionReview;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ExamEndpointsTests
{
    private static readonly string[] InProgressForbiddenPatterns =
    [
        "\"isCorrect\"",
        "\"correctAnswerOptionId\"",
        "\"explanation\"",
        "\"score\"",
        "\"percentage\"",
        "\"passed\""
    ];

    private static readonly string[] GlobalForbiddenPatterns =
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
        "\"examVersion\""
    ];

    private static readonly string[] ExamAccessForbiddenPatterns =
    [
        "examAccessGrant",
        "grantId",
        "nurseProfileId",
        "paymentOrder",
        "paymentProduct",
        "checkout",
        "provider",
        "productId"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ExamEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("GET", "/api/v1/exams")]
    [InlineData("GET", "/api/v1/exams/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/v1/exams/11111111-1111-1111-1111-111111111111/sessions")]
    [InlineData("GET", "/api/v1/exam-sessions/11111111-1111-1111-1111-111111111111")]
    [InlineData("PUT", "/api/v1/exam-sessions/11111111-1111-1111-1111-111111111111/answers")]
    [InlineData("POST", "/api/v1/exam-sessions/11111111-1111-1111-1111-111111111111/submit")]
    [InlineData("GET", "/api/v1/exam-sessions/11111111-1111-1111-1111-111111111111/result")]
    [InlineData("GET", "/api/v1/exam-sessions/11111111-1111-1111-1111-111111111111/review")]
    [InlineData("GET", "/api/v1/me/nurse-profile/exam-attempts")]
    public async Task ExamEndpoints_WithoutJwt_ReturnUnauthorized(string method, string path)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method is "POST" or "PUT"
                ? new StringContent("{}", Encoding.UTF8, "application/json")
                : null
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListExams_SendsQueryWithPaginationAndFilters()
    {
        var countryId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListExamsQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.CountryId == countryId && q.CategoryId == categoryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ExamCatalogItemDto>
            {
                Page = 2,
                PageSize = 5,
                TotalCount = 1,
                Items = [new ExamCatalogItemDto { Id = Guid.NewGuid(), Title = "NCLEX RN", CountryName = "United States" }]
            });

        var response = await _client.GetAsync($"/api/v1/exams?page=2&pageSize=5&countryId={countryId}&categoryId={categoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StartExamSession_WithForbiddenAccess_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Exam access is required."));

        var response = await _client.PostAsync($"/api/v1/exams/{Guid.NewGuid()}/sessions", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StartExamSession_WithForbiddenAccess_ResponseDoesNotExposePaymentOrGrantInternals()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Exam access is required."));

        var response = await _client.PostAsync($"/api/v1/exams/{Guid.NewGuid()}/sessions", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ExamAccessForbiddenPatterns);
    }

    [Fact]
    public async Task StartExamSession_WithAuthorizedAccess_ReturnsExistingSessionContract()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var examId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<StartExamSessionCommand>(c => c.ExamId == examId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamSessionDto
            {
                Id = Guid.NewGuid(),
                ExamId = examId,
                ExamTitle = "NCLEX RN",
                Status = "InProgress",
                Items =
                [
                    new ExamSessionQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Question",
                        Options = [new ExamSessionAnswerOptionDto { Id = Guid.NewGuid(), Text = "A" }]
                    }
                ]
            });

        var response = await _client.PostAsync($"/api/v1/exams/{examId}/sessions", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("examTitle", json, StringComparison.OrdinalIgnoreCase);
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
        AssertDoesNotContain(json, ExamAccessForbiddenPatterns);
    }

    [Fact]
    public async Task GetExamSession_WhenHidden_ReturnsNotFound()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetExamSessionQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Exam session was not found."));

        var response = await _client.GetAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SaveAnswers_WithValidationFailure_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<SaveExamSessionAnswersCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Answers", "At least one answer is required.")]));

        var response = await _client.PutAsJsonAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}/answers", new { answers = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SubmitExamSession_WithInvalidTransition_ReturnsConflict()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<SubmitExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Only in-progress exam sessions can be submitted."));

        var response = await _client.PostAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}/submit", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetExamSession_ReturnsInProgressJsonWithSelectedAnswerOptionId()
    {
        var selectedOptionId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetExamSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamSessionDto
            {
                Id = Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                ExamTitle = "NCLEX RN",
                Status = "InProgress",
                Items =
                [
                    new ExamSessionQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Question",
                        SelectedExamSessionAnswerOptionId = selectedOptionId,
                        Options = [new ExamSessionAnswerOptionDto { Id = selectedOptionId, Text = "A" }]
                    }
                ]
            });

        var response = await _client.GetAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("selectedExamSessionAnswerOptionId", json, StringComparison.OrdinalIgnoreCase);
        AssertDoesNotContain(json, InProgressForbiddenPatterns);
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
    }

    [Fact]
    public async Task GetExamSessionReview_ReturnsCompletedJsonWithExplanationsOnlyAfterCompletion()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetExamSessionReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamSessionReviewDto
            {
                Id = Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                ExamTitle = "NCLEX RN",
                Status = "Submitted",
                Items =
                [
                    new ExamSessionReviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Question",
                        Explanation = "Because safety comes first.",
                        CorrectAnswerOptionId = Guid.NewGuid(),
                        Options = [new ExamSessionReviewOptionDto { Id = Guid.NewGuid(), Text = "A", IsCorrect = true }]
                    }
                ]
            });

        var response = await _client.GetAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}/review");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("explanation", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("correctAnswerOptionId", json, StringComparison.OrdinalIgnoreCase);
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
    }

    [Fact]
    public async Task ListMyExamAttempts_ReturnsPaginatedSafeJson()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyExamAttemptsQuery>(q => q.Page == 2 && q.PageSize == 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ExamAttemptDto>
            {
                Page = 2,
                PageSize = 5,
                TotalCount = 6,
                Items = [new ExamAttemptDto { Id = Guid.NewGuid(), ExamId = Guid.NewGuid(), ExamTitle = "NCLEX RN", Status = "Submitted" }]
            });

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/exam-attempts?page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
        var body = JsonSerializer.Deserialize<PaginatedResult<ExamAttemptDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
    }

    [Fact]
    public async Task ExamEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetExamQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamDetailDto { Id = Guid.NewGuid(), Title = "NCLEX RN", CountryName = "United States" });

        var response = await _client.GetAsync($"/api/v1/exams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static void AssertDoesNotContain(string json, IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
