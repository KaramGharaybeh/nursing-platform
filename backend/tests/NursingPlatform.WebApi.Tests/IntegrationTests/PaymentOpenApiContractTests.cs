using System.Net;
using System.Text.Json;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PaymentOpenApiContractTests
{
    private readonly HttpClient _client;

    public PaymentOpenApiContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_DocumentsPaymentResponseContractsAndHeaders()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        AssertResponseContract(
            paths,
            "/api/v1/me/nurse-profile/payment/orders",
            "post",
            "201",
            "PaymentOrderDto",
            "Location");
        AssertResponseContract(
            paths,
            "/api/v1/me/nurse-profile/payment/orders/{orderId}/checkout",
            "post",
            "200",
            "PaymentCheckoutSessionDto",
            "Cache-Control");
        AssertResponseContract(
            paths,
            "/api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete",
            "post",
            "200",
            "PaymentCompletionDto",
            "Cache-Control");
    }

    private static void AssertResponseContract(
        JsonElement paths,
        string path,
        string method,
        string status,
        string schema,
        string header)
    {
        var response = paths
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("responses")
            .GetProperty(status);

        Assert.Equal(
            $"#/components/schemas/{schema}",
            response
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema")
                .GetProperty("$ref")
                .GetString());
        Assert.True(response.GetProperty("headers").TryGetProperty(header, out _));
    }
}
