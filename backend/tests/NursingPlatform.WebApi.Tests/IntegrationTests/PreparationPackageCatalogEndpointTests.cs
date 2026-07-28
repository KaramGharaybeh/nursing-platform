using System.Net;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Catalog;
using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PreparationPackageCatalogEndpointTests
{
    private static readonly string[] ForbiddenCatalogJsonPatterns =
    [
        "passwordHash",
        "accessToken",
        "refreshToken",
        "tokenHash",
        "secret",
        "questionText",
        "answerKey",
        "answerOptionId",
        "correctAnswerOptionId",
        "isCorrect",
        "rationale",
        "explanation",
        "score",
        "percentage",
        "passed",
        "report",
        "authorization",
        "permission",
        "role",
        "paymentProviderId",
        "providerPaymentIntentId",
        "providerCheckoutSessionId",
        "examVersion",
        "practiceCollectionVersion",
        "reportingProfilePublication"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public PreparationPackageCatalogEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCatalogOffers_AllowsAnonymousAndReturnsSafeJson()
    {
        var countryId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<ListPreparationPackageOffersQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.CountryId == countryId && q.ExamCategoryId == categoryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<PreparationPackageOfferListItemDto>
            {
                Items = [CreateOfferListItem()],
                Page = 2,
                PageSize = 5,
                TotalCount = 6
            });

        var response = await _client.GetAsync($"/api/v1/preparation-packages/offers?page=2&pageSize=5&countryId={countryId}&examCategoryId={categoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenCatalogJsonPatterns);
        var body = JsonSerializer.Deserialize<PaginatedResult<PreparationPackageOfferListItemDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
        Assert.Equal(2, body.TotalPages);
        Assert.Single(body.Items);
    }

    [Fact]
    public async Task GetCatalogOffer_DoesNotExposePasswordHashTokensProtectedExamContentCorrectAnswersOrInternalAuthorizationState()
    {
        _senderMock
            .Setup(s => s.Send(It.Is<GetPreparationPackageOfferQuery>(q => q.Slug == "nclex-rn-complete"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PreparationPackageOfferDetailDto
            {
                Id = Guid.NewGuid(),
                Title = "NCLEX RN Complete",
                Slug = "nclex-rn-complete",
                Summary = "Preparation package",
                CountryId = Guid.NewGuid(),
                CountryName = "United States",
                ExamCategoryId = Guid.NewGuid(),
                ExamCategoryName = "NCLEX",
                ExamId = Guid.NewGuid(),
                ExamTitle = "NCLEX RN",
                MaterialCount = 3,
                PracticeItemCount = 20,
                AccessDurationDays = 90,
                PriceAmountMinor = 9900,
                Currency = "USD",
                Components =
                [
                    new PreparationPackageCatalogComponentSummaryDto { Name = "Study materials", Count = 3, Summary = "Published study material versions included in this package." }
                ]
            });

        var response = await _client.GetAsync("/api/v1/preparation-packages/offers/nclex-rn-complete");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotContain(json, ForbiddenCatalogJsonPatterns);
        var body = JsonSerializer.Deserialize<PreparationPackageOfferDetailDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("nclex-rn-complete", body.Slug);
    }

    private static PreparationPackageOfferListItemDto CreateOfferListItem()
    {
        return new PreparationPackageOfferListItemDto
        {
            Id = Guid.NewGuid(),
            Title = "NCLEX RN Complete",
            Slug = "nclex-rn-complete",
            Summary = "Preparation package",
            CountryId = Guid.NewGuid(),
            CountryName = "United States",
            ExamCategoryId = Guid.NewGuid(),
            ExamCategoryName = "NCLEX",
            ExamId = Guid.NewGuid(),
            ExamTitle = "NCLEX RN",
            MaterialCount = 3,
            PracticeItemCount = 20,
            AccessDurationDays = 90,
            PriceAmountMinor = 9900,
            Currency = "USD"
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
