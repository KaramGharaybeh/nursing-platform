using System.Net;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;
using NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;
using NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PackageExamSessionEndpointTests
{
    private const string RouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/entitlements/{0}/exam-session";
    private const string StateRouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/entitlements/{0}/exam-session";

    private static readonly string[] ForbiddenResponsePatterns =
    [
        "PackageBenefitRightId",
        "InternalRight",
        "benefitRightId",
        "rightId",
        "examSessionProvenance",
        "provenanceId",
        "CorrectOption",
        "CorrectAnswer",
        "AnswerKey",
        "Rationale",
        "PaymentProvider",
        "ProviderSession",
        "providerSecret",
        "AccessToken",
        "RefreshToken",
        "PasswordHash",
        "reportEvidence",
        "internalAuthorizationState"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PackageExamSessionEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartPackageExamSession_WhenUnauthenticated_Returns401()
    {
        var entitlementId = Guid.NewGuid();

        var response = await _client.PostAsync(string.Format(RouteTemplate, entitlementId), null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenEntitlementMissing_Returns404()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package entitlement was not found."));

        var response = await _client.PostAsync(string.Format(RouteTemplate, Guid.NewGuid()), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenEntitlementOwnedByAnotherNurse_Returns404()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package entitlement was not found."));

        var response = await _client.PostAsync(string.Format(RouteTemplate, Guid.NewGuid()), null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("owned", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenEntitlementInactive_Returns409WithCode()
    {
        await AssertConflictCodeAsync("package-entitlement-inactive");
    }

    [Fact]
    public async Task StartPackageExamSession_WhenAttemptRightMissing_Returns409WithCode()
    {
        await AssertConflictCodeAsync("package-attempt-right-missing");
    }

    [Fact]
    public async Task StartPackageExamSession_WhenAttemptRightConsumed_Returns409WithCode()
    {
        await AssertConflictCodeAsync("package-attempt-consumed");
    }

    [Fact]
    public async Task StartPackageExamSession_WhenExamVersionUnavailable_Returns409WithCode()
    {
        await AssertConflictCodeAsync("package-exam-version-unavailable");
    }

    [Fact]
    public async Task StartPackageExamSession_WhenExistingDifferentSourceSession_Returns409WithCode()
    {
        await AssertConflictCodeAsync("exam-session-source-conflict");
    }

    [Fact]
    public async Task StartPackageExamSession_WhenValid_ReturnsSuccessWithSafeDto()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var dto = CreateStartDto(entitlementId);
        _senderMock
            .Setup(s => s.Send(It.Is<StartPackageExamSessionCommand>(c => c.EntitlementId == entitlementId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _client.PostAsync(string.Format(RouteTemplate, entitlementId), null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenResponsePatterns);
        var body = JsonSerializer.Deserialize<PackageExamSessionStartDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(entitlementId, body.EntitlementId);
        Assert.Equal("PackageAttempt", body.Source);
        Assert.Equal(dto.Session.Id, body.Session.Id);
        Assert.Equal("PackageAttempt", body.Session.Source);
    }

    [Fact]
    public async Task StartPackageExamSession_Response_DoesNotExposeInternalRightOrSensitiveFields()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStartDto(entitlementId));

        var response = await _client.PostAsync(string.Format(RouteTemplate, entitlementId), null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenResponsePatterns);
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("answerKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rationale", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartPackageExamSession_WhenSameEntitlementRetried_ReturnsExistingSessionSafely()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStartDto(entitlementId, sessionId));

        var first = await _client.PostAsync(string.Format(RouteTemplate, entitlementId), null);
        var second = await _client.PostAsync(string.Format(RouteTemplate, entitlementId), null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var firstJson = await first.Content.ReadAsStringAsync();
        var secondJson = await second.Content.ReadAsStringAsync();
        AssertDoesNotContain(firstJson, ForbiddenResponsePatterns);
        AssertDoesNotContain(secondJson, ForbiddenResponsePatterns);
        var firstBody = JsonSerializer.Deserialize<PackageExamSessionStartDto>(firstJson, NurseEndpointTestAuth.JsonOptions);
        var secondBody = JsonSerializer.Deserialize<PackageExamSessionStartDto>(secondJson, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(sessionId, firstBody.Session.Id);
        Assert.Equal(firstBody.Session.Id, secondBody.Session.Id);
    }

    [Theory]
    [InlineData("POST", "/api/v1/admin/preparation-package/entitlements/11111111-1111-1111-1111-111111111111/exam-session")]
    [InlineData("POST", "/api/v1/admin/preparation-package/offers/11111111-1111-1111-1111-111111111111/exam-session")]
    public async Task PackageExamStartEndpoint_IsNotAvailableUnderAdminRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("POST", "/api/v1/employer/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/exam-session")]
    [InlineData("POST", "/api/v1/employer/preparation-packages/purchases/11111111-1111-1111-1111-111111111111/exam-session")]
    public async Task PackageExamStartEndpoint_IsNotAvailableUnderEmployerRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("POST", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/reports/generate")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/reports")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/workspace")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/workspace/dashboard")]
    [InlineData("POST", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/attempt-reset")]
    public async Task Stage3EndpointScope_DoesNotExposeReportWorkspaceEmployerOrAttemptResetRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertConflictCodeAsync(string code)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartPackageExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PackageExamSessionConflictException(code, "Package exam session conflict."));

        var response = await _client.PostAsync(string.Format(RouteTemplate, Guid.NewGuid()), null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(code, document.RootElement.GetProperty("code").GetString());
    }

    private static PackageExamSessionStartDto CreateStartDto(Guid entitlementId, Guid? sessionId = null)
    {
        return new PackageExamSessionStartDto
        {
            Session = new ExamSessionDto
            {
                Id = sessionId ?? Guid.NewGuid(),
                ExamId = Guid.NewGuid(),
                ExamTitle = "NCLEX RN",
                Status = "InProgress",
                Source = "PackageAttempt",
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RemainingSeconds = 3600,
                Items =
                [
                    new ExamSessionQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Question",
                        Options = [new ExamSessionAnswerOptionDto { Id = Guid.NewGuid(), Text = "A" }]
                    }
                ]
            },
            Source = "PackageAttempt",
            EntitlementId = entitlementId,
            IncludedExamId = Guid.NewGuid(),
            IncludedExamVersionId = Guid.NewGuid(),
            PreparationPackageOfferId = Guid.NewGuid(),
            AccessEndsAt = DateTime.UtcNow.AddDays(30)
        };
    }

    private static void AssertDoesNotContain(string json, IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task GetMyPackageExamSessionState_WhenUnauthenticated_Returns401()
    {
        var response = await _client.GetAsync(string.Format(StateRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState.GetMyPackageExamSessionStateQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMyPackageExamSessionState_WhenOwnedWithSession_ReturnsStateWithoutProvenance()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(
                It.Is<NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState.GetMyPackageExamSessionStateQuery>(
                    q => q.EntitlementId == entitlementId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs.PackageExamSessionStateDto
            {
                HasSession = true,
                SessionId = sessionId,
                ExamId = Guid.NewGuid(),
                Status = "InProgress",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            });

        var response = await _client.GetAsync(string.Format(StateRouteTemplate, entitlementId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenResponsePatterns);
        Assert.DoesNotContain("PackageBenefitRightId", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs.PackageExamSessionStateDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.True(body.HasSession);
        Assert.Equal(sessionId, body.SessionId);
    }

    [Fact]
    public async Task GetMyPackageExamSessionState_WhenEntitlementForeign_Returns404WithoutOwnershipExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<NursingPlatform.Application.PreparationPackages.ExamSessions.GetMyPackageExamSessionState.GetMyPackageExamSessionStateQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package exam session was not found."));

        var response = await _client.GetAsync(string.Format(StateRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
    }
}
