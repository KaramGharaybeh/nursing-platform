using System.Net;
using System.Text.Json;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Application.PreparationPackages.Reports.DTOs;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class OpenApiNumericContractTests
{
    private readonly HttpClient _client;

    public OpenApiNumericContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_UsesJsonNumberTypesForNumericDtoProperties()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        AssertSingleType(schemas, "PaginatedResultOfPreparationPackageOfferListItemDto", "page", "integer");
        AssertSingleType(schemas, "PaginatedResultOfPreparationPackageOfferListItemDto", "pageSize", "integer");
        AssertSingleType(schemas, "PaginatedResultOfPreparationPackageOfferListItemDto", "totalCount", "integer");
        AssertSingleType(schemas, "PaginatedResultOfPreparationPackageOfferListItemDto", "totalPages", "integer");
        AssertSingleType(schemas, "PreparationPackageOfferListItemDto", "priceAmountMinor", "string");
        AssertSingleType(schemas, "PaymentCheckoutSessionDto", "amountMinor", "string");
        AssertSingleType(schemas, "PaymentOrderDto", "totalAmountMinor", "string");
        AssertSingleType(schemas, "CreateAdminPaymentProductRequest", "unitAmountMinor", "string");
        AssertSingleType(schemas, "PaymentOrderItemDto", "lineTotalAmountMinor", "string");
        AssertSingleType(schemas, "CreateAdminPreparationPackageOfferRequest", "priceAmountMinor", "string");
        AssertSingleType(schemas, "PackageEntitlementSnapshotDto", "priceAmountMinor", "string");
        AssertSingleType(schemas, "PackagePracticeProgressSummaryDto", "totalItems", "integer");
        AssertSingleType(schemas, "PackagePracticeProgressSummaryDto", "correctCount", "integer");
        AssertSingleType(schemas, "PackageAnalyticalReportDto", "score", "integer");
        AssertSingleType(schemas, "PackageAnalyticalReportDto", "percentage", "number");
        AssertSingleType(schemas, "PackageAnalyticalReportTopicResultDto", "percentage", "number");
    }

    [Fact]
    public void RuntimeJson_EmitsFinancialMinorUnitsAsPrecisionSafeStringsAndOtherNumericValuesAsNumbers()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var paginationJson = JsonSerializer.Serialize(new PaginatedResult<PreparationPackageOfferListItemDto>
        {
            Items = [],
            Page = 2,
            PageSize = 20,
            TotalCount = 40
        }, jsonOptions);
        var paymentJson = JsonSerializer.Serialize(new PaymentCheckoutSessionDto
        {
            Id = Guid.NewGuid(),
            PaymentOrderId = Guid.NewGuid(),
            AmountMinor = 9007199254740991L,
            Currency = "USD"
        }, jsonOptions);
        var reportJson = JsonSerializer.Serialize(new PackageAnalyticalReportDto
        {
            Score = 80,
            MaxScore = 100,
            Percentage = 80.5m,
            CorrectCount = 8,
            QuestionCount = 10
        }, jsonOptions);

        using var pagination = JsonDocument.Parse(paginationJson);
        using var payment = JsonDocument.Parse(paymentJson);
        using var report = JsonDocument.Parse(reportJson);

        Assert.Equal(JsonValueKind.Number, pagination.RootElement.GetProperty("page").ValueKind);
        Assert.Equal(JsonValueKind.Number, pagination.RootElement.GetProperty("totalCount").ValueKind);
        Assert.Equal(JsonValueKind.String, payment.RootElement.GetProperty("amountMinor").ValueKind);
        Assert.Equal("9007199254740991", payment.RootElement.GetProperty("amountMinor").GetString());
        Assert.Equal(JsonValueKind.Number, report.RootElement.GetProperty("percentage").ValueKind);
        Assert.Equal(80.5m, report.RootElement.GetProperty("percentage").GetDecimal());
    }

    [Theory]
    [InlineData("{\"amountMinor\":\"9223372036854775807\"}")]
    [InlineData("{\"amountMinor\":9223372036854775807}")]
    public void RuntimeJson_ReadsFinancialMinorUnitsFromStringAndLegacyNumber(string json)
    {
        var value = JsonSerializer.Deserialize<PaymentCheckoutSessionDto>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(value);
        Assert.Equal(long.MaxValue, value.AmountMinor);
    }

    private static void AssertSingleType(
        JsonElement schemas,
        string schemaName,
        string propertyName,
        string expectedType)
    {
        var type = schemas
            .GetProperty(schemaName)
            .GetProperty("properties")
            .GetProperty(propertyName)
            .GetProperty("type");

        Assert.Equal(JsonValueKind.String, type.ValueKind);
        Assert.Equal(expectedType, type.GetString());
    }
}
