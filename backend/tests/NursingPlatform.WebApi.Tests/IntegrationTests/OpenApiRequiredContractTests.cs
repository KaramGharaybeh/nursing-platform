using System.Net;
using System.Text.Json;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class OpenApiRequiredContractTests
{
    private readonly HttpClient _client;

    public OpenApiRequiredContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_UsesNullableReferenceAndValueTypeMetadataForRequiredProperties()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        AssertRequired(
            schemas,
            "PreparationPackageOfferListItemDto",
            "id",
            "title",
            "slug",
            "countryId",
            "countryName",
            "examCategoryId",
            "examCategoryName",
            "examId",
            "examTitle",
            "materialCount",
            "practiceItemCount",
            "accessDurationDays",
            "priceAmountMinor",
            "currency");
        AssertOptional(schemas, "PreparationPackageOfferListItemDto", "summary");

        AssertRequired(schemas, "PackageEntitlementDetailDto", "purchasedSnapshot", "id", "benefitRights");
        AssertRequired(schemas, "PaymentCheckoutSessionDto", "id", "status", "currency", "amountMinor", "expiresAt");
        AssertOptional(schemas, "PaymentCheckoutSessionDto", "checkoutUrl");
        AssertOptional(schemas, "PaymentCompletionDto", "paidAt");
    }

    private static void AssertRequired(JsonElement schemas, string schemaName, params string[] properties)
    {
        var required = GetRequiredProperties(schemas, schemaName);

        foreach (var property in properties)
        {
            Assert.Contains(property, required);
        }
    }

    private static void AssertOptional(JsonElement schemas, string schemaName, params string[] properties)
    {
        var required = GetRequiredProperties(schemas, schemaName);

        foreach (var property in properties)
        {
            Assert.DoesNotContain(property, required);
        }
    }

    private static HashSet<string> GetRequiredProperties(JsonElement schemas, string schemaName)
    {
        var schema = schemas.GetProperty(schemaName);

        return schema.TryGetProperty("required", out var required)
            ? required.EnumerateArray().Select(item => item.GetString()!).ToHashSet(StringComparer.Ordinal)
            : [];
    }
}
