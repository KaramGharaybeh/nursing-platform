using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.PreparationPackages.PracticeProgress;
using NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PackagePracticeProgressEndpointTests
{
    private const string ProgressRouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/entitlements/{0}/practice-progress";
    private const string SubmitRouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/entitlements/{0}/practice-progress/items/{1}/answer";
    private const string ItemsRouteTemplate = "/api/v1/me/nurse-profile/preparation-packages/entitlements/{0}/practice-progress/items";

    private static readonly string[] ForbiddenPracticeProgressJsonPatterns =
    [
        "ExamQuestionId",
        "ExamAnswerOptionId",
        "ExamSessionId",
        "ExamSession",
        "QuestionTextSnapshot",
        "OptionTextSnapshot",
        "CorrectAnswer",
        "CorrectOption",
        "IsCorrect",
        "AnswerKey",
        "Rationale",
        "ExplanationSnapshot",
        "BenefitRightId",
        "PackageBenefitRightId",
        "nurseProfileId",
        "internalAuthorizationState",
        "passwordHash",
        "accessToken",
        "refreshToken",
        "tokenHash",
        "secret"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PackagePracticeProgressEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPackagePracticeProgress_WhenUnauthenticated_Returns401()
    {
        var response = await _client.GetAsync(string.Format(ProgressRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPackagePracticeProgressQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitPackagePracticeAnswer_WhenUnauthenticated_Returns401()
    {
        var request = new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync(
            string.Format(SubmitRouteTemplate, Guid.NewGuid(), Guid.NewGuid()),
            request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<SubmitPackagePracticeAnswerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPackagePracticeProgress_WhenOwnedByCurrentNurse_ReturnsProgressSummary()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var dto = CreateProgressSummary(entitlementId);
        _senderMock
            .Setup(s => s.Send(It.Is<GetPackagePracticeProgressQuery>(q => q.EntitlementId == entitlementId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _client.GetAsync(string.Format(ProgressRouteTemplate, entitlementId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
        var body = JsonSerializer.Deserialize<PackagePracticeProgressSummaryDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(entitlementId, body.PackagePurchaseEntitlementId);
        Assert.Equal(2, body.TotalItems);
        Assert.Equal(1, body.AnsweredCount);
        Assert.Equal(1, body.UnansweredCount);
        Assert.Equal(1, body.CorrectCount);
        Assert.Equal(0, body.IncorrectCount);
        Assert.Equal(2, body.ItemStates.Count);
    }

    [Fact]
    public async Task SubmitPackagePracticeAnswer_WhenOwnedByCurrentNurse_ReturnsSubmissionResult()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var practiceItemId = Guid.NewGuid();
        var selectedOptionId = Guid.NewGuid();
        var dto = new PackagePracticeAnswerSubmissionDto
        {
            PracticeItemId = practiceItemId,
            State = PackagePracticeProgressItemState.AnsweredCorrect,
            SelectedPracticeAnswerOptionId = selectedOptionId,
            LastAnsweredAt = DateTime.UtcNow
        };
        _senderMock
            .Setup(s => s.Send(
                It.Is<SubmitPackagePracticeAnswerCommand>(command => command.EntitlementId == entitlementId
                    && command.PracticeItemId == practiceItemId
                    && command.Request.SelectedPracticeAnswerOptionId == selectedOptionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _client.PostAsJsonAsync(
            string.Format(SubmitRouteTemplate, entitlementId, practiceItemId),
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = selectedOptionId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
        var body = JsonSerializer.Deserialize<PackagePracticeAnswerSubmissionDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(practiceItemId, body.PracticeItemId);
        Assert.Equal(PackagePracticeProgressItemState.AnsweredCorrect, body.State);
        Assert.Equal(selectedOptionId, body.SelectedPracticeAnswerOptionId);
    }

    [Fact]
    public async Task GetPackagePracticeProgress_WhenEntitlementOwnedByAnotherNurse_Returns404WithoutOwnershipExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPackagePracticeProgressQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package practice progress was not found."));

        var response = await _client.GetAsync(string.Format(ProgressRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("owned", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitPackagePracticeAnswer_WhenEntitlementOwnedByAnotherNurse_Returns404WithoutOwnershipExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<SubmitPackagePracticeAnswerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package practice progress was not found."));

        var response = await _client.PostAsJsonAsync(
            string.Format(SubmitRouteTemplate, Guid.NewGuid(), Guid.NewGuid()),
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("owned", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPackagePracticeProgress_WhenEntitlementExpiredButOwned_ReturnsHistoricalProgress()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<GetPackagePracticeProgressQuery>(q => q.EntitlementId == entitlementId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProgressSummary(entitlementId));

        var response = await _client.GetAsync(string.Format(ProgressRouteTemplate, entitlementId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
        var body = JsonSerializer.Deserialize<PackagePracticeProgressSummaryDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(entitlementId, body.PackagePurchaseEntitlementId);
    }

    [Fact]
    public async Task SubmitPackagePracticeAnswer_WhenEntitlementExpired_Returns409()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<SubmitPackagePracticeAnswerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Package practice access is not available."));

        var response = await _client.PostAsJsonAsync(
            string.Format(SubmitRouteTemplate, Guid.NewGuid(), Guid.NewGuid()),
            new SubmitPackagePracticeAnswerRequest { SelectedPracticeAnswerOptionId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
    }

    [Fact]
    public async Task PackagePracticeProgressResponses_DoNotExposeOfficialExamFieldsOrInternalBenefitRightIds()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetPackagePracticeProgressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProgressSummary(entitlementId));

        var response = await _client.GetAsync(string.Format(ProgressRouteTemplate, entitlementId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
    }

    [Theory]
    [InlineData("GET", "/api/v1/admin/preparation-package/entitlements/11111111-1111-1111-1111-111111111111/practice-progress")]
    [InlineData("POST", "/api/v1/admin/preparation-package/entitlements/11111111-1111-1111-1111-111111111111/practice-progress/items/22222222-2222-2222-2222-222222222222/answer")]
    [InlineData("GET", "/api/v1/employer/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/practice-progress")]
    [InlineData("POST", "/api/v1/employer/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/practice-progress/items/22222222-2222-2222-2222-222222222222/answer")]
    public async Task PackagePracticeProgressEndpoint_IsNotAvailableUnderAdminOrEmployerRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPackagePracticeProgressQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        _senderMock.Verify(s => s.Send(It.IsAny<SubmitPackagePracticeAnswerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PackagePracticeProgressSummaryDto CreateProgressSummary(Guid entitlementId)
    {
        var answeredItemId = Guid.NewGuid();
        return new PackagePracticeProgressSummaryDto
        {
            PackagePurchaseEntitlementId = entitlementId,
            PracticeCollectionVersionId = Guid.NewGuid(),
            TotalItems = 2,
            AnsweredCount = 1,
            UnansweredCount = 1,
            CorrectCount = 1,
            IncorrectCount = 0,
            ItemStates =
            [
                new PackagePracticeProgressItemStateDto
                {
                    PracticeItemId = answeredItemId,
                    State = PackagePracticeProgressItemState.AnsweredCorrect,
                    SelectedPracticeAnswerOptionId = Guid.NewGuid(),
                    LastAnsweredAt = DateTime.UtcNow
                },
                new PackagePracticeProgressItemStateDto
                {
                    PracticeItemId = Guid.NewGuid(),
                    State = PackagePracticeProgressItemState.Unanswered
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

    [Fact]
    public async Task GetPackagePracticeItems_WhenUnauthenticated_Returns401()
    {
        var response = await _client.GetAsync(string.Format(ItemsRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPackagePracticeItems_WhenOwnedByCurrentNurse_ReturnsLearnerContentWithoutAnswerKey()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var entitlementId = Guid.NewGuid();
        var dto = new NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeContentListDto
        {
            PackagePurchaseEntitlementId = entitlementId,
            PracticeCollectionVersionId = Guid.NewGuid(),
            TotalItems = 1,
            Items =
            [
                new NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeItemContentDto
                {
                    PracticeItemId = Guid.NewGuid(),
                    DisplayOrder = 1,
                    Prompt = "Practice prompt",
                    AnswerOptions =
                    [
                        new NursingPlatform.Application.PreparationPackages.PracticeProgress.DTOs.PackagePracticeAnswerOptionContentDto
                        {
                            PracticeAnswerOptionId = Guid.NewGuid(),
                            OptionText = "Option A",
                            DisplayOrder = 1
                        }
                    ]
                }
            ]
        };
        _senderMock
            .Setup(s => s.Send(It.Is<NursingPlatform.Application.PreparationPackages.PracticeProgress.GetPackagePracticeItemsQuery>(q => q.EntitlementId == entitlementId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _client.GetAsync(string.Format(ItemsRouteTemplate, entitlementId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenPracticeProgressJsonPatterns);
        Assert.DoesNotContain("IsCorrect", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ImmediateFeedback", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPackagePracticeItems_WhenEntitlementOwnedByAnotherNurse_Returns404WithoutOwnershipExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<NursingPlatform.Application.PreparationPackages.PracticeProgress.GetPackagePracticeItemsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package practice items were not found."));

        var response = await _client.GetAsync(string.Format(ItemsRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPackagePracticeItems_WhenPracticeAccessUnavailable_Returns409()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<NursingPlatform.Application.PreparationPackages.PracticeProgress.GetPackagePracticeItemsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Package practice access is not available."));

        var response = await _client.GetAsync(string.Format(ItemsRouteTemplate, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
