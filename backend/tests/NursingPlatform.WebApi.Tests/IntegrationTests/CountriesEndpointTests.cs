using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.ReferenceData.Countries.DTOs;
using NursingPlatform.Application.ReferenceData.Countries.Queries.ListCountries;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class CountriesEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public CountriesEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListCountries_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/countries");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListCountries_WithJwt_ReturnsOkWithCountryList()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CountryListItemDto { Id = Guid.NewGuid(), Name = "Canada", Code = "CA" },
                new CountryListItemDto { Id = Guid.NewGuid(), Name = "Saudi Arabia", Code = "SA" }
            ]);

        var response = await _client.GetAsync("/api/v1/countries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal("Canada", items[0].GetProperty("name").GetString());
        Assert.Equal("CA", items[0].GetProperty("code").GetString());
        Assert.True(items[0].TryGetProperty("id", out _));
    }

    [Fact]
    public async Task DevelopmentOpenApi_ListCountries_PublishesTypedResponseAndRequiresBearer()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/v1/countries")
            .GetProperty("get");

        var schema = operation.GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");

        Assert.Equal("array", schema.GetProperty("type").GetString());
        Assert.Equal(
            "#/components/schemas/CountryListItemDto",
            schema.GetProperty("items").GetProperty("$ref").GetString());
        Assert.Contains("bearer", operation.GetProperty("security")[0].ToString(), StringComparison.OrdinalIgnoreCase);

        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("CountryListItemDto", out var countryDto));
        var properties = countryDto.GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(["id", "name", "code"], properties);
    }
}
