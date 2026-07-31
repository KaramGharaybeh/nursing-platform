using System.Net;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.PreparationPackages.Reports.DTOs;
using NursingPlatform.Application.PreparationPackages.Reports.Generation;
using NursingPlatform.Application.PreparationPackages.Reports.GetPackageAnalyticalReport;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PackageAnalyticalReportEndpointTests
{
    private const string RouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/exam-sessions/{0}/report";

    private static readonly string[] ForbiddenReportJsonPatterns =
    [
        "passwordHash",
        "accessToken",
        "refreshToken",
        "tokenHash",
        "secret",
        "nurseProfileId",
        "packageEntitlementId",
        "packageBenefitRightId",
        "benefitRightId",
        "rightId",
        "paymentOrderItemId",
        "purchasedOfferSnapshotId",
        "examSessionProvenance",
        "provenanceId",
        "correctAnswerOptionId",
        "answerKey",
        "rationale",
        "internalAuthorizationState",
        "providerPaymentIntentId",
        "providerCheckoutSessionId"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PackageAnalyticalReportEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPackageAnalyticalReport_WhenUnauthenticated_Returns401()
    {
        var sessionId = Guid.NewGuid();

        var response = await _client.GetAsync(string.Format(RouteTemplate, sessionId));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPackageAnalyticalReportQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPackageAnalyticalReport_WhenSessionMissingOrOwnedByAnotherNurse_Returns404WithoutOwnershipExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPackageAnalyticalReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package exam session was not found."));

        var response = await _client.GetAsync(string.Format(RouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("owned", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("package-report-session-not-finalized")]
    [InlineData("package-report-session-not-qualified")]
    [InlineData("package-report-provenance-invalid")]
    [InlineData("package-report-right-missing")]
    [InlineData("package-report-profile-incomplete")]
    public async Task GetPackageAnalyticalReport_WhenApplicationRejectsReportGeneration_Returns409WithCode(string code)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPackageAnalyticalReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PackageReportConflictException(code, "Package analytical report conflict."));

        var response = await _client.GetAsync(string.Format(RouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(code, document.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetPackageAnalyticalReport_WhenValidFinalizedPackageAttempt_ReturnsSafeReportDto()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var sessionId = Guid.NewGuid();
        var dto = CreateReportDto(sessionId);
        _senderMock
            .Setup(s => s.Send(It.Is<GetPackageAnalyticalReportQuery>(q => q.SessionId == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _client.GetAsync(string.Format(RouteTemplate, sessionId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenReportJsonPatterns);
        var body = JsonSerializer.Deserialize<PackageAnalyticalReportDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(dto.Id, body.Id);
        Assert.Equal(sessionId, body.ExamSessionId);
        Assert.Equal(8, body.Score);
        Assert.Equal(10, body.MaxScore);
        Assert.Equal(80m, body.Percentage);
        Assert.True(body.Passed);
        Assert.Single(body.TopicResults);
        Assert.Single(body.GuidanceItems);
    }

    [Fact]
    public async Task GetPackageAnalyticalReport_WhenRequestedRepeatedly_ReturnsSameReportIdempotently()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var sessionId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<GetPackageAnalyticalReportQuery>(q => q.SessionId == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateReportDto(sessionId, reportId));

        var first = await _client.GetAsync(string.Format(RouteTemplate, sessionId));
        var second = await _client.GetAsync(string.Format(RouteTemplate, sessionId));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var firstJson = await first.Content.ReadAsStringAsync();
        var secondJson = await second.Content.ReadAsStringAsync();
        AssertDoesNotContain(firstJson, ForbiddenReportJsonPatterns);
        AssertDoesNotContain(secondJson, ForbiddenReportJsonPatterns);
        var firstBody = JsonSerializer.Deserialize<PackageAnalyticalReportDto>(firstJson, NurseEndpointTestAuth.JsonOptions);
        var secondBody = JsonSerializer.Deserialize<PackageAnalyticalReportDto>(secondJson, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(reportId, firstBody.Id);
        Assert.Equal(firstBody.Id, secondBody.Id);
        Assert.Equal(sessionId, secondBody.ExamSessionId);
    }

    [Theory]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/reports")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/reports")]
    [InlineData("POST", "/api/v1/me/nurse-profile/preparation-packages/exam-sessions/11111111-1111-1111-1111-111111111111/report/generate")]
    [InlineData("GET", "/api/v1/admin/preparation-package/exam-sessions/11111111-1111-1111-1111-111111111111/report")]
    [InlineData("GET", "/api/v1/employer/preparation-packages/exam-sessions/11111111-1111-1111-1111-111111111111/report")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/workspace/dashboard")]
    public async Task PackageAnalyticalReportEndpointScope_DoesNotExposeListAdminEmployerWorkspaceOrManualGenerationRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPackageAnalyticalReportQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PackageAnalyticalReportDto CreateReportDto(Guid sessionId, Guid? reportId = null)
    {
        var topicId = Guid.NewGuid();
        return new PackageAnalyticalReportDto
        {
            Id = reportId ?? Guid.NewGuid(),
            ExamSessionId = sessionId,
            GeneratedAt = DateTime.UtcNow,
            FinalizedSessionStatus = ExamSessionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow.AddMinutes(-5),
            FinalizedAt = DateTime.UtcNow,
            Score = 8,
            MaxScore = 10,
            Percentage = 80m,
            Passed = true,
            CorrectCount = 8,
            QuestionCount = 10,
            TopicResults =
            [
                new PackageAnalyticalReportTopicResultDto
                {
                    ReportingTopicId = topicId,
                    TopicName = "Safe practice",
                    TopicDescription = "Medication safety and prioritization.",
                    ScoredQuestionCount = 10,
                    CorrectCount = 8,
                    EarnedPoints = 8,
                    AvailablePoints = 10,
                    Percentage = 80m,
                    SortOrder = 1
                }
            ],
            GuidanceItems =
            [
                new PackageAnalyticalReportGuidanceItemDto
                {
                    ReportingTopicId = topicId,
                    SourceType = "StudyMaterial",
                    SourceVersionId = Guid.NewGuid(),
                    Title = "Review medication safety",
                    SourceMetadata = "module-1",
                    SortOrder = 1
                }
            ]
        };
    }

    private static void AssertDoesNotContain(string json, IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
