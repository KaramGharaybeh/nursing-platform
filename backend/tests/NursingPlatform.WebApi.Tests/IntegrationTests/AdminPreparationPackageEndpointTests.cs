using System.Net;
using System.Net.Http.Json;
using System.Text;
using MediatR;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.DTOs;

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
