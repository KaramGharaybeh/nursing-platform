using System.Net;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class LocalDevelopmentCorsTests
{
    private readonly WebApiTestFactory _factory;

    public LocalDevelopmentCorsTests(WebApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DevelopmentCors_AllowsAngularDevelopmentOriginForApiRequests()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type,authorization");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Equal(["http://localhost:4200"], origins);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Headers", out var headers));
        Assert.Contains("content-type", string.Join(',', headers), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("authorization", string.Join(',', headers), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProductionCors_DoesNotAllowAngularDevelopmentOrigin()
    {
        using var productionClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        }).CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await productionClient.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
