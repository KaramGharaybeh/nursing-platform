using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseLanguageEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseLanguageEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListLanguages_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/languages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Languages_GetAndPut_WithNurseJwt_ReturnsDtos()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var languageId = Guid.NewGuid();
        var dto = new NurseLanguageDto
        {
            Id = Guid.NewGuid(),
            LanguageId = languageId,
            Name = "English",
            Code = "en",
            Proficiency = "Fluent"
        };
        _senderMock.Setup(s => s.Send(It.IsAny<UpdateNurseLanguagesCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);
        _senderMock.Setup(s => s.Send(It.IsAny<ListCurrentNurseLanguagesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);

        var putResponse = await _client.PutAsJsonAsync("/api/v1/me/nurse-profile/languages", new
        {
            languages = new[] { new { languageId, proficiency = "Fluent" } }
        });
        var getResponse = await _client.GetAsync("/api/v1/me/nurse-profile/languages");

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var json = await getResponse.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<List<NurseLanguageDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(languageId, Assert.Single(body).LanguageId);
    }

    [Fact]
    public async Task PutLanguages_WithDuplicateLanguageIds_ReturnsBadRequest()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var languageId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateNurseLanguagesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Languages", "Duplicate language IDs are not allowed.")]));

        var response = await _client.PutAsJsonAsync("/api/v1/me/nurse-profile/languages", new
        {
            languages = new[]
            {
                new { languageId, proficiency = "Fluent" },
                new { languageId, proficiency = "Native" }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
