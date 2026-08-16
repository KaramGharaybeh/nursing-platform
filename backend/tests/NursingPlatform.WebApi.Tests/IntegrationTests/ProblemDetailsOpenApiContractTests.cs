using System.Net;
using System.Text.Json;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ProblemDetailsOpenApiContractTests
{
    private readonly HttpClient _client;

    public ProblemDetailsOpenApiContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_DocumentsTypedProblemDetailsContractsAndRetryHeader()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        AssertSchema(schemas, "ProblemDetails", ["type", "title", "status", "detail", "traceId"]);
        AssertSchema(schemas, "ValidationProblemDetails", ["type", "title", "status", "detail", "traceId", "errors"]);
        AssertSchema(schemas, "CodedProblemDetails", ["type", "title", "status", "detail", "traceId", "code"]);
        AssertSchema(schemas, "RetryableProblemDetails", ["type", "title", "status", "detail", "traceId", "retryAfterSeconds"]);
        AssertMissingProperties(schemas, "ProblemDetails", "errors", "code", "retryAfterSeconds");
        AssertMissingProperties(schemas, "ValidationProblemDetails", "code", "retryAfterSeconds");
        AssertMissingProperties(schemas, "CodedProblemDetails", "errors", "retryAfterSeconds");
        AssertMissingProperties(schemas, "RetryableProblemDetails", "errors", "code");

        AssertResponseSchema(
            document.RootElement,
            "/api/v1/me/nurse-profile/payment/orders/{orderId}/checkout",
            "post",
            "409",
            "RetryableProblemDetails");
        var checkoutConflict = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/v1/me/nurse-profile/payment/orders/{orderId}/checkout")
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("409");
        Assert.True(checkoutConflict.GetProperty("headers").TryGetProperty("Retry-After", out _));

        AssertResponseSchema(
            document.RootElement,
            "/api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session",
            "post",
            "409",
            "CodedProblemDetails");
        AssertResponseSchema(
            document.RootElement,
            "/api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report",
            "get",
            "409",
            "CodedProblemDetails");
        AssertResponseSchema(
            document.RootElement,
            "/api/v1/preparation-packages/offers",
            "get",
            "400",
            "ValidationProblemDetails");
        AssertResponseSchema(
            document.RootElement,
            "/api/v1/me/nurse-profile/payment/orders",
            "post",
            "400",
            "ValidationProblemDetails");
    }

    private static void AssertSchema(JsonElement schemas, string schemaName, string[] requiredProperties)
    {
        var schema = schemas.GetProperty(schemaName);
        var properties = schema.GetProperty("properties");
        var required = schema.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToHashSet(StringComparer.Ordinal);

        foreach (var property in requiredProperties)
        {
            Assert.True(properties.TryGetProperty(property, out _), $"{schemaName}.{property} is missing.");
            Assert.Contains(property, required);
        }
    }

    private static void AssertResponseSchema(
        JsonElement document,
        string path,
        string method,
        string status,
        string schemaName)
    {
        var schemaReference = document
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("responses")
            .GetProperty(status)
            .GetProperty("content")
            .GetProperty("application/problem+json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal($"#/components/schemas/{schemaName}", schemaReference);
    }

    private static void AssertMissingProperties(JsonElement schemas, string schemaName, params string[] propertyNames)
    {
        var properties = schemas.GetProperty(schemaName).GetProperty("properties");

        foreach (var propertyName in propertyNames)
        {
            Assert.False(
                properties.TryGetProperty(propertyName, out _),
                $"{schemaName}.{propertyName} must not be present.");
        }
    }
}
