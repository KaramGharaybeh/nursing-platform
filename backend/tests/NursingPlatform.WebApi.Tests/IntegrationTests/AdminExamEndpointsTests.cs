using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Admin.AnswerOptions;
using NursingPlatform.Application.Exams.Admin.Categories;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Application.Exams.Admin.Exams;
using NursingPlatform.Application.Exams.Admin.Questions;
using NursingPlatform.Application.Exams.Admin.Versions;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Exams.Queries.GetExamSession;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class AdminExamEndpointsTests
{
    private static readonly (string Method, string Path)[] AdminEndpoints =
    [
        ("GET", "/api/v1/admin/exam-categories"),
        ("GET", "/api/v1/admin/exam-categories/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/exam-categories"),
        ("PUT", "/api/v1/admin/exam-categories/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/exam-categories/11111111-1111-1111-1111-111111111111/archive"),
        ("POST", "/api/v1/admin/exam-categories/11111111-1111-1111-1111-111111111111/restore"),
        ("DELETE", "/api/v1/admin/exam-categories/11111111-1111-1111-1111-111111111111"),
        ("GET", "/api/v1/admin/exams"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/exams"),
        ("PUT", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/archive"),
        ("DELETE", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/validate"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/publish"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/retire"),
        ("DELETE", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions"),
        ("PUT", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/deactivate"),
        ("DELETE", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333"),
        ("GET", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/options"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/options"),
        ("PUT", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/options/44444444-4444-4444-4444-444444444444"),
        ("POST", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/options/44444444-4444-4444-4444-444444444444/deactivate"),
        ("DELETE", "/api/v1/admin/exams/11111111-1111-1111-1111-111111111111/versions/22222222-2222-2222-2222-222222222222/questions/33333333-3333-3333-3333-333333333333/options/44444444-4444-4444-4444-444444444444")
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
        "\"examSession\""
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public AdminExamEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _permissionServiceMock = factory.PermissionServiceMock;
        _senderMock.Reset();
        _permissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [MemberData(nameof(AdminEndpointData))]
    public async Task AdminExamEndpoints_WithoutJwt_ReturnUnauthorized(string method, string path)
    {
        var response = await _client.SendAsync(CreateRequest(method, path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AdminEndpointData))]
    public async Task AdminExamEndpoints_WithoutPermission_ReturnForbidden(string method, string path)
    {
        var userId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var response = await _client.SendAsync(CreateRequest(method, path));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminListExamCategories_WithViewPermission_SendsQuery()
    {
        AuthorizeWith(Permissions.Exams.View);
        _senderMock
            .Setup(s => s.Send(It.Is<ListAdminExamCategoriesQuery>(q => q.Page == 2 && q.PageSize == 5 && q.IsActive == true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<AdminExamCategoryDto> { Page = 2, PageSize = 5, Items = [] });

        var response = await _client.GetAsync("/api/v1/admin/exam-categories?page=2&pageSize=5&isActive=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminCreateExam_WithCreatePermission_ReturnsCreated()
    {
        AuthorizeWith(Permissions.Exams.Create);
        var examId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateAdminExamCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamDto { Id = examId, Title = "NCLEX", CountryName = "United States" });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/exams", new
        {
            countryId = Guid.NewGuid(),
            title = "NCLEX",
            slug = "nclex",
            durationMinutes = 60,
            passingScorePercentage = 70,
            isFree = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AdminUpdateExam_WithEditPermission_SendsCommand()
    {
        AuthorizeWith(Permissions.Exams.Edit);
        var examId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<UpdateAdminExamCommand>(c => c.Id == examId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamDto { Id = examId, Title = "Updated", CountryName = "United States" });

        var response = await _client.PutAsJsonAsync($"/api/v1/admin/exams/{examId}", new
        {
            countryId = Guid.NewGuid(),
            title = "Updated",
            slug = "updated",
            durationMinutes = 60,
            passingScorePercentage = 70,
            isFree = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminUpdateDraftVersionEndpoint_IsNotMapped_WhenNoEditableFieldsExist()
    {
        AuthorizeWith(Permissions.Exams.Edit);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}",
            new { });

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task AdminDeleteExam_WithDeletePermission_SendsCommand()
    {
        AuthorizeWith(Permissions.Exams.Delete);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteAdminExamCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/v1/admin/exams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AdminPublishDraftVersion_WithQuestionsManagePermission_SendsCommand()
    {
        AuthorizeWith(Permissions.Questions.Manage);
        _senderMock
            .Setup(s => s.Send(It.IsAny<PublishAdminDraftExamVersionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamVersionDto { Id = Guid.NewGuid(), Status = "Published" });

        var response = await _client.PostAsync($"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminDeleteDraftVersion_WithExamsDeletePermission_SendsCommand()
    {
        AuthorizeWith(Permissions.Exams.Delete);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteAdminDraftExamVersionCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task AdminQuestionEndpoints_WithQuestionsViewOrManagePermission_Succeed()
    {
        AuthorizeWith(Permissions.Questions.Manage, Permissions.Questions.View);
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateAdminExamQuestionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamQuestionDto { Id = Guid.NewGuid(), QuestionText = "Q" });

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/questions",
            new { questionText = "Q", questionType = 0, points = 1, displayOrder = 1, isActive = true });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AdminAnswerOptionEndpoints_WithQuestionsManagePermission_Succeed()
    {
        AuthorizeWith(Permissions.Questions.Manage);
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateAdminExamAnswerOptionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamAnswerOptionDto { Id = Guid.NewGuid(), OptionText = "A" });

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/questions/{Guid.NewGuid()}/options",
            new { optionText = "A", displayOrder = 1, isCorrect = true, isActive = true });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AdminContentValidationFailure_ReturnsValidationProblemDetails()
    {
        AuthorizeWith(Permissions.Exams.Create);
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateAdminExamCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Title", "Title is required.")]));

        var response = await _client.PostAsJsonAsync("/api/v1/admin/exams", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AdminContentConflict_ReturnsConflict()
    {
        AuthorizeWith(Permissions.Exams.Edit);
        _senderMock
            .Setup(s => s.Send(It.IsAny<ArchiveAdminExamCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Conflict"));

        var response = await _client.PostAsync($"/api/v1/admin/exams/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AdminContentNotFound_ReturnsNotFound()
    {
        AuthorizeWith(Permissions.Exams.View);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAdminExamQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var response = await _client.GetAsync($"/api/v1/admin/exams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminContentJson_DoesNotExposeGlobalForbiddenFields()
    {
        AuthorizeWith(Permissions.Exams.View);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAdminExamQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamDto { Id = Guid.NewGuid(), Title = "NCLEX", CountryName = "United States" });

        var response = await _client.GetAsync($"/api/v1/admin/exams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
    }

    [Fact]
    public async Task AdminQuestionJson_MayExposeCorrectnessOnlyOnAdminRoutes()
    {
        AuthorizeWith(Permissions.Questions.View);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAdminExamQuestionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminExamQuestionDto
            {
                Id = Guid.NewGuid(),
                QuestionText = "Q",
                Explanation = "Because.",
                Options = [new AdminExamAnswerOptionDto { Id = Guid.NewGuid(), OptionText = "A", IsCorrect = true }]
            });

        var response = await _client.GetAsync($"/api/v1/admin/exams/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/questions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("explanation", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isCorrect", json, StringComparison.OrdinalIgnoreCase);
        AssertDoesNotContain(json, GlobalForbiddenPatterns);
    }

    [Fact]
    public async Task NurseInProgressExamSessionJson_StillDoesNotExposeCorrectAnswersOrScoring()
    {
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
                        Options = [new ExamSessionAnswerOptionDto { Id = Guid.NewGuid(), Text = "A" }]
                    }
                ]
            });

        var response = await _client.GetAsync($"/api/v1/exam-sessions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"isCorrect\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"correctAnswerOptionId\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"explanation\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"score\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"percentage\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"passed\"", json, StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<object[]> AdminEndpointData()
    {
        return AdminEndpoints.Select(endpoint => new object[] { endpoint.Method, endpoint.Path });
    }

    private void AuthorizeWith(params string[] permissions)
    {
        var userId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions.ToHashSet());
    }

    private static HttpRequestMessage CreateRequest(string method, string path)
    {
        return new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method is "POST" or "PUT"
                ? new StringContent("{}", Encoding.UTF8, "application/json")
                : null
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
