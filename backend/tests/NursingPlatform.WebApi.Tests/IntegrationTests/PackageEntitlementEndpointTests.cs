using System.Net;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Entitlements.DTOs;
using NursingPlatform.Application.PreparationPackages.Entitlements.GetMyPackageEntitlement;
using NursingPlatform.Application.PreparationPackages.Entitlements.ListMyPackageEntitlements;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PackageEntitlementEndpointTests
{
    private static readonly string[] ForbiddenEntitlementJsonPatterns =
    [
        "passwordHash",
        "accessToken",
        "refreshToken",
        "tokenHash",
        "secret",
        "nurseProfileId",
        "paymentOrderItemId",
        "purchasedOfferSnapshotId",
        "benefitRightId",
        "packageBenefitRightId",
        "rightId",
        "questionText",
        "answerKey",
        "correctAnswerOptionId",
        "isCorrect",
        "providerPaymentIntentId",
        "providerCheckoutSessionId"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PackageEntitlementEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListMyPackageEntitlements_Returns401WithoutJwt()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/preparation-packages/entitlements");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListMyPackageEntitlementsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListMyPackageEntitlements_ReturnsOnlyCurrentNurseEntitlements()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyPackageEntitlementsQuery>(q => q.Page == 2 && q.PageSize == 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<PackageEntitlementListItemDto>
            {
                Items = [CreateEntitlementListItem()],
                Page = 2,
                PageSize = 5,
                TotalCount = 6
            });

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/preparation-packages/entitlements?page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenEntitlementJsonPatterns);
        var body = JsonSerializer.Deserialize<PaginatedResult<PackageEntitlementListItemDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
        Assert.Equal(2, body.TotalPages);
        Assert.Single(body.Items);
    }

    [Fact]
    public async Task GetMyPackageEntitlement_ForAnotherNurse_Returns404Or403WithoutExposure()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyPackageEntitlementQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Package entitlement was not found."));

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/preparation-packages/entitlements/{Guid.NewGuid()}");

        Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("nurseProfileId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("another nurse", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMyPackageEntitlement_RawJsonDoesNotContainForbiddenSensitiveFields()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyPackageEntitlementQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEntitlementDetail());

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/preparation-packages/entitlements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenEntitlementJsonPatterns);
        var body = JsonSerializer.Deserialize<PackageEntitlementDetailDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(4, body.BenefitRights.Count);
    }

    [Theory]
    [InlineData("POST", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/attempts")]
    [InlineData("POST", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/reports/generate")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/reports")]
    public async Task PackageStage2EndpointScope_DoesNotMapPackageAttemptStartReportGenerationOrReportAccessRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("POST", "/api/v1/employer/preparation-packages/purchases")]
    [InlineData("GET", "/api/v1/employer/preparation-packages/reports")]
    [InlineData("GET", "/api/v1/me/nurse-profile/preparation-packages/entitlements/11111111-1111-1111-1111-111111111111/practice-progress")]
    public async Task PackageStage2EndpointScope_DoesNotMapEmployerPackagePurchaseReportOrPracticeProgressRoutes(string method, string path)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExistingPaymentAndExamEndpoints_RemainBackwardCompatibleAfterPackageFulfillment()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<NursingPlatform.Application.Payments.Queries.ListPaymentProducts.ListPaymentProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<NursingPlatform.Application.Payments.DTOs.PaymentProductDto> { Items = [], Page = 1, PageSize = 20 });
        _senderMock
            .Setup(s => s.Send(It.IsAny<NursingPlatform.Application.Exams.Queries.ListExams.ListExamsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<NursingPlatform.Application.Exams.DTOs.ExamCatalogItemDto> { Items = [], Page = 1, PageSize = 20 });

        var paymentResponse = await _client.GetAsync("/api/v1/payment/products");
        var examResponse = await _client.GetAsync("/api/v1/exams");

        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, examResponse.StatusCode);
    }

    private static PackageEntitlementListItemDto CreateEntitlementListItem()
    {
        return new PackageEntitlementListItemDto
        {
            Id = Guid.NewGuid(),
            PackageOfferId = Guid.NewGuid(),
            PackageOfferTitle = "NCLEX RN Complete",
            PackageDefinitionId = Guid.NewGuid(),
            PackageDefinitionTitle = "NCLEX RN Complete",
            PackageVersionId = Guid.NewGuid(),
            IncludedExamId = Guid.NewGuid(),
            IncludedExamTitle = "NCLEX RN",
            AccessStartsAt = DateTime.UtcNow,
            AccessEndsAt = DateTime.UtcNow.AddDays(90),
            Status = "Active"
        };
    }

    private static PackageEntitlementDetailDto CreateEntitlementDetail()
    {
        var item = CreateEntitlementListItem();
        return new PackageEntitlementDetailDto
        {
            Id = item.Id,
            PackageOfferId = item.PackageOfferId,
            PackageOfferTitle = item.PackageOfferTitle,
            PackageDefinitionId = item.PackageDefinitionId,
            PackageDefinitionTitle = item.PackageDefinitionTitle,
            PackageVersionId = item.PackageVersionId,
            IncludedExamId = item.IncludedExamId,
            IncludedExamTitle = item.IncludedExamTitle,
            AccessStartsAt = item.AccessStartsAt,
            AccessEndsAt = item.AccessEndsAt,
            Status = item.Status,
            PurchasedSnapshot = new PackageEntitlementSnapshotDto
            {
                PackageOfferSlug = "nclex-rn-complete",
                PackageDefinitionSlug = "nclex-rn-complete",
                PriceAmountMinor = 9900,
                Currency = "USD",
                AccessDurationDays = 90
            },
            BenefitRights =
            [
                new PackageBenefitRightSummaryDto { RightType = "MaterialsAccess", Status = "Available" },
                new PackageBenefitRightSummaryDto { RightType = "PracticeAccess", Status = "Available" },
                new PackageBenefitRightSummaryDto { RightType = "PackageExamAttemptEligibility", Status = "Available" },
                new PackageBenefitRightSummaryDto { RightType = "ReportEligibility", Status = "Dormant" }
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
