using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class AdminPreparationPackageEndpointTests
{
    private static readonly (string Method, string Path)[] ForbiddenLaterStageRoutes =
    [
        ("GET", "/api/v1/admin/preparation-package/employer-reports"),
        ("GET", "/api/v1/admin/preparation-package/reports"),
        ("GET", "/api/v1/admin/preparation-package/practice-progress"),
        ("GET", "/api/v1/admin/preparation-package/purchase-history"),
        ("GET", "/api/v1/preparation-packages/workspace"),
        ("POST", "/api/v1/preparation-packages/offers/11111111-1111-1111-1111-111111111111/purchase"),
        ("POST", "/api/v1/preparation-packages/attempts"),
        ("GET", "/api/v1/me/employer-profile/preparation-package/reports")
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public AdminPreparationPackageEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _permissionServiceMock = factory.PermissionServiceMock;
        _senderMock.Reset();
        _permissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminReportingTopicCreate_Returns401WithoutJwt()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-topics", new
        {
            examCategoryId = Guid.NewGuid(),
            name = "Safety",
            slug = "safety"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminReportingTopicCreate_Returns403WithoutReportingTopicsManagePermission()
    {
        AuthorizeWith();

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-topics", new
        {
            examCategoryId = Guid.NewGuid(),
            name = "Safety",
            slug = "safety"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminReportingTopicCreate_Returns201WithReportingTopicsManagePermission()
    {
        AuthorizeWith(Permissions.ReportingTopics.Manage);
        var topicId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateAdminReportingTopicCommand>(c => c.Request.Name == "Safety"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminReportingTopicDto { Id = topicId, Name = "Safety", Slug = "safety", ExamCategoryName = "NCLEX", IsActive = true });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-topics", new
        {
            examCategoryId = Guid.NewGuid(),
            name = "Safety",
            slug = "safety"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/admin/preparation-package/reporting-topics/{topicId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AdminReportingProfileCreate_Returns401WithoutJwt()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-profiles", CreateReportingProfileRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminReportingProfileCreate_Returns403WithoutReportingProfilesManagePermission()
    {
        AuthorizeWith();

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-profiles", CreateReportingProfileRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminReportingProfileCreate_Returns201WithReportingProfilesManagePermission()
    {
        AuthorizeWith(Permissions.ReportingProfiles.Manage);
        var profileId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateAdminReportingProfileCommand>(c => c.Request.Name == "NCLEX reporting profile"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminReportingProfilePublicationDto
            {
                Id = profileId,
                ExamVersionId = Guid.NewGuid(),
                Name = "NCLEX reporting profile",
                Status = "Draft"
            });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-profiles", CreateReportingProfileRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/admin/preparation-package/reporting-profiles/{profileId}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData(Permissions.Exams.Edit)]
    [InlineData(Permissions.Questions.Manage)]
    public async Task AdminReportingProfileCreate_WithExamOrQuestionPermissionOnly_Returns403(string permission)
    {
        AuthorizeWith(permission);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/reporting-profiles", CreateReportingProfileRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialCreate_Returns401WithoutJwt()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/materials", CreateStudyMaterialRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialCreate_Returns403WithoutStudyMaterialsManagePermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/materials", CreateStudyMaterialRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialCreate_Returns201WithStudyMaterialsManagePermission()
    {
        AuthorizeWith(Permissions.StudyMaterials.Manage);
        var materialId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateAdminStudyMaterialCommand>(c => c.Request.Title == "Clinical Safety Guide"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminStudyMaterialDto { Id = materialId, Title = "Clinical Safety Guide", Slug = "clinical-safety-guide", Description = "Medication and safety review." });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/materials", CreateStudyMaterialRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/admin/preparation-package/materials/{materialId}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData(Permissions.Exams.Edit)]
    [InlineData(Permissions.Questions.Manage)]
    public async Task AdminStudyMaterialCreate_WithExamOrQuestionPermissionOnly_Returns403(string permission)
    {
        AuthorizeWith(permission);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/materials", CreateStudyMaterialRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionUpdate_Returns401WithoutJwt()
    {
        var response = await _client.PutAsJsonAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}", CreateStudyMaterialVersionRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionUpdate_Returns403WithoutStudyMaterialsManagePermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PutAsJsonAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}", CreateStudyMaterialVersionRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Permissions.Exams.Edit)]
    [InlineData(Permissions.Questions.Manage)]
    public async Task AdminStudyMaterialVersionUpdate_WithExamOrQuestionPermissionOnly_Returns403(string permission)
    {
        AuthorizeWith(permission);

        var response = await _client.PutAsJsonAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}", CreateStudyMaterialVersionRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionUpdate_Returns200WithStudyMaterialsManagePermission()
    {
        var materialId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        AuthorizeWith(Permissions.StudyMaterials.Manage);
        _senderMock
            .Setup(s => s.Send(It.Is<UpdateAdminStudyMaterialVersionCommand>(c => c.StudyMaterialId == materialId && c.VersionId == versionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStudyMaterialVersionDto(materialId, versionId, "Draft"));

        var response = await _client.PutAsJsonAsync($"/api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}", CreateStudyMaterialVersionRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionPublish_Returns401WithoutJwt()
    {
        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/publish", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionPublish_Returns403WithoutStudyMaterialsManagePermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/publish", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionPublish_Returns200WithStudyMaterialsManagePermission()
    {
        var materialId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        AuthorizeWith(Permissions.StudyMaterials.Manage);
        _senderMock
            .Setup(s => s.Send(It.Is<PublishAdminStudyMaterialVersionCommand>(c => c.StudyMaterialId == materialId && c.VersionId == versionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStudyMaterialVersionDto(materialId, versionId, "Published"));

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionRetire_Returns401WithoutJwt()
    {
        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/retire", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionRetire_Returns403WithoutStudyMaterialsManagePermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/retire", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStudyMaterialVersionRetire_Returns200WithStudyMaterialsManagePermission()
    {
        var materialId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        AuthorizeWith(Permissions.StudyMaterials.Manage);
        _senderMock
            .Setup(s => s.Send(It.Is<RetireAdminStudyMaterialVersionCommand>(c => c.StudyMaterialId == materialId && c.VersionId == versionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStudyMaterialVersionDto(materialId, versionId, "Retired"));

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}/retire", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminPracticeCollectionCreate_Returns401WithoutJwt()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/practice-collections", CreatePracticeCollectionRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminPracticeCollectionCreate_Returns403WithoutPracticeCollectionsManagePermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/practice-collections", CreatePracticeCollectionRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminPracticeCollectionCreate_Returns201WithPracticeCollectionsManagePermission()
    {
        AuthorizeWith(Permissions.PracticeCollections.Manage);
        var collectionId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateAdminPracticeCollectionCommand>(c => c.Request.Title == "Practice Safety Set"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminPracticeCollectionDto { Id = collectionId, Title = "Practice Safety Set", Slug = "practice-safety-set", Description = "Independent safety practice." });

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/practice-collections", CreatePracticeCollectionRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/admin/preparation-package/practice-collections/{collectionId}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData(Permissions.Exams.Edit)]
    [InlineData(Permissions.Questions.Manage)]
    public async Task AdminPracticeCollectionCreate_WithExamOrQuestionPermissionOnly_Returns403(string permission)
    {
        AuthorizeWith(permission);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/preparation-package/practice-collections", CreatePracticeCollectionRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminPackageVersionPublish_Returns403WithoutPreparationPackagesPublishPermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage);

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/packages/{Guid.NewGuid()}/versions/{Guid.NewGuid()}/publish", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminPackageVersionPublish_Returns200WithPreparationPackagesPublishPermission()
    {
        AuthorizeWith(Permissions.PreparationPackages.Publish);
        var packageId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<PublishAdminPreparationPackageVersionCommand>(c => c.PreparationPackageDefinitionId == packageId && c.VersionId == versionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminPreparationPackageVersionDto { Id = versionId, PreparationPackageDefinitionId = packageId, Status = "Published" });

        var response = await _client.PostAsync($"/api/v1/admin/preparation-package/packages/{packageId}/versions/{versionId}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminOfferActivate_EnforcesPreparationPackageOffersManagePermission()
    {
        var offerId = Guid.NewGuid();
        AuthorizeWith(Permissions.PreparationPackages.Publish);

        var forbiddenResponse = await _client.PostAsync($"/api/v1/admin/preparation-package/offers/{offerId}/activate", null);

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        _senderMock.Reset();
        _permissionServiceMock.Reset();
        AuthorizeWith(Permissions.PreparationPackageOffers.Manage);
        _senderMock
            .Setup(s => s.Send(It.Is<ActivateAdminPreparationPackageOfferCommand>(c => c.Id == offerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminPreparationPackageOfferDto { Id = offerId, Status = "Active", Title = "Package", Slug = "package", Currency = "USD", AccessDurationDays = 90 });

        var okResponse = await _client.PostAsync($"/api/v1/admin/preparation-package/offers/{offerId}/activate", null);

        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
    }

    [Fact]
    public async Task AdminPracticeCollectionVersionCreate_DoesNotExposeOfficialExamIdentifiersOrSnapshotsInJson()
    {
        AuthorizeWith(Permissions.PracticeCollections.Manage);
        var collectionId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var topicId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateAdminPracticeCollectionVersionCommand>(c => c.PracticeCollectionId == collectionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminPracticeCollectionVersionDto
            {
                Id = versionId,
                PracticeCollectionId = collectionId,
                VersionNumber = 1,
                Status = "Draft",
                Items =
                [
                    new AdminPracticeItemDto
                    {
                        Id = Guid.NewGuid(),
                        ReportingTopicId = topicId,
                        Prompt = "Independent practice prompt",
                        ImmediateFeedback = "Independent practice feedback",
                        DisplayOrder = 1,
                        Options =
                        [
                            new AdminPracticeAnswerOptionDto { Id = Guid.NewGuid(), OptionText = "Practice option A", DisplayOrder = 1, IsCorrect = true },
                            new AdminPracticeAnswerOptionDto { Id = Guid.NewGuid(), OptionText = "Practice option B", DisplayOrder = 2, IsCorrect = false }
                        ]
                    }
                ]
            });

        var response = await _client.PostAsJsonAsync($"/api/v1/admin/preparation-package/practice-collections/{collectionId}/versions", new
        {
            items = new[]
            {
                new
                {
                    reportingTopicId = topicId,
                    prompt = "Independent practice prompt",
                    immediateFeedback = "Independent practice feedback",
                    displayOrder = 1,
                    answerOptions = new[]
                    {
                        new { optionText = "Practice option A", isCorrect = true, displayOrder = 1 },
                        new { optionText = "Practice option B", isCorrect = false, displayOrder = 2 }
                    }
                }
            }
        });

        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("examQuestionId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("examAnswerOptionId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("examSessionId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("questionTextSnapshot", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("optionTextSnapshot", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanationSnapshot", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("answerKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rationale", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correctAnswer", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correctOption", json, StringComparison.OrdinalIgnoreCase);

        var body = JsonSerializer.Deserialize<AdminPracticeCollectionVersionDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(body);
        Assert.Equal(versionId, body.Id);
        Assert.Equal(topicId, Assert.Single(body.Items).ReportingTopicId);
    }

    [Theory]
    [MemberData(nameof(ForbiddenLaterStageRouteData))]
    public async Task AdminEndpoints_DoNotAcceptEmployerPackageReportPracticeProgressOrPurchaseHistoryRoutes(string method, string path)
    {
        AuthorizeWith(Permissions.PreparationPackages.Manage, Permissions.PreparationPackageOffers.Manage, Permissions.ReportingProfiles.Manage);

        var response = await _client.SendAsync(CreateRequest(method, path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ExistingExamCatalogAndStartEndpoints_RemainBackwardCompatible()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListExamsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ExamCatalogItemDto> { Items = [], Page = 1, PageSize = 20 });
        _senderMock
            .Setup(s => s.Send(It.IsAny<StartExamSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamSessionDto { Id = Guid.NewGuid(), ExamId = Guid.NewGuid(), ExamTitle = "NCLEX RN", Status = "InProgress" });

        var listResponse = await _client.GetAsync("/api/v1/exams");
        var startResponse = await _client.PostAsync($"/api/v1/exams/{Guid.NewGuid()}/sessions", null);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);
    }

    public static IEnumerable<object[]> ForbiddenLaterStageRouteData()
    {
        return ForbiddenLaterStageRoutes.Select(route => new object[] { route.Method, route.Path });
    }

    private void AuthorizeWith(params string[] permissions)
    {
        var userId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions.ToHashSet());
    }

    private static object CreateStudyMaterialRequest() => new
    {
        title = "Clinical Safety Guide",
        slug = "clinical-safety-guide",
        description = "Medication and safety review."
    };

    private static object CreateStudyMaterialVersionRequest() => new
    {
        materialType = StudyMaterialType.FormattedText,
        formattedTextContent = "Updated clinical safety content.",
        reportingTopicIds = new[] { Guid.NewGuid() }
    };

    private static AdminStudyMaterialVersionDto CreateStudyMaterialVersionDto(Guid materialId, Guid versionId, string status) => new()
    {
        Id = versionId,
        StudyMaterialId = materialId,
        VersionNumber = 1,
        MaterialType = "FormattedText",
        Status = status,
        FormattedTextContent = "Updated clinical safety content.",
        ReportingTopicIds = [Guid.NewGuid()]
    };

    private static object CreatePracticeCollectionRequest() => new
    {
        title = "Practice Safety Set",
        slug = "practice-safety-set",
        description = "Independent safety practice."
    };

    private static object CreateReportingProfileRequest() => new
    {
        examVersionId = Guid.NewGuid(),
        name = "NCLEX reporting profile"
    };

    private static HttpRequestMessage CreateRequest(string method, string path)
    {
        return new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method is "POST" or "PUT"
                ? new StringContent("{}", Encoding.UTF8, "application/json")
                : null
        };
    }
}
