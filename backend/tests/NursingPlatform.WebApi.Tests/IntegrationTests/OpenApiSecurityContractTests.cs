using System.Net;
using System.Text.Json;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class OpenApiSecurityContractTests
{
    private readonly HttpClient _client;

    public OpenApiSecurityContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_DocumentsBearerSecurityOnlyForProtectedOperations()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        AssertBearerProtected(paths, "/api/v1/me/nurse-profile/preparation-packages/entitlements", "get");
        AssertBearerProtected(paths, "/api/v1/me", "get");
        AssertBearerProtected(paths, "/api/v1/admin/preparation-package/reporting-topics", "get");
        AssertBearerProtected(paths, "/api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete", "post");
        AssertEmptyBearerChallengeResponse(paths, "/api/v1/me/nurse-profile/preparation-packages/entitlements", "get");
        AssertEmptyBearerChallengeResponse(paths, "/api/v1/me", "get");

        var publicCatalog = paths
            .GetProperty("/api/v1/preparation-packages/offers")
            .GetProperty("get");
        Assert.False(publicCatalog.TryGetProperty("security", out _));
    }

    private static void AssertBearerProtected(JsonElement paths, string path, string method)
    {
        var security = paths
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("security");

        Assert.Contains(
            security.EnumerateArray(),
            requirement => requirement.TryGetProperty("Bearer", out _));
    }

    private static void AssertEmptyBearerChallengeResponse(
        JsonElement paths,
        string path,
        string method)
    {
        var response = paths
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("responses")
            .GetProperty("401");

        Assert.False(response.TryGetProperty("content", out _));
        Assert.True(response.GetProperty("headers").TryGetProperty("WWW-Authenticate", out _));
    }
}
