using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseSkillEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseSkillEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListSkills_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/skills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Skills_GetAndPut_ResponseJsonDoesNotContainNormalizedName()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var dto = new NurseSkillDto { Id = Guid.NewGuid(), Name = "Critical Care" };
        _senderMock.Setup(s => s.Send(It.IsAny<UpdateNurseSkillsCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);
        _senderMock.Setup(s => s.Send(It.IsAny<ListCurrentNurseSkillsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);

        var putResponse = await _client.PutAsJsonAsync("/api/v1/me/nurse-profile/skills", new { skills = new[] { "Critical Care" } });
        var getResponse = await _client.GetAsync("/api/v1/me/nurse-profile/skills");

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var json = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("normalizedName", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<List<NurseSkillDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("Critical Care", Assert.Single(body).Name);
    }

    [Fact]
    public async Task PutSkills_WithNormalizedDuplicateNames_ReturnsBadRequest()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateNurseSkillsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Skills", "Duplicate skill names are not allowed.")]));

        var response = await _client.PutAsJsonAsync("/api/v1/me/nurse-profile/skills", new
        {
            skills = new[] { "Critical   Care", " critical care " }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
