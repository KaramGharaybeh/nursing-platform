using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseExperience;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseExperience;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseExperienceEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseExperienceEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListExperiences_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/experiences");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Experience_CreateListUpdateDelete_WorksForOwnedRecords()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var experienceId = Guid.NewGuid();
        var dto = new NurseExperienceDto
        {
            Id = experienceId,
            FacilityName = "City Hospital",
            JobTitle = "Registered Nurse",
            StartDate = new DateOnly(2021, 1, 1),
            IsCurrent = true,
            Description = "ICU"
        };
        _senderMock.Setup(s => s.Send(It.IsAny<CreateNurseExperienceCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<ListCurrentNurseExperiencesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);
        _senderMock.Setup(s => s.Send(It.IsAny<UpdateNurseExperienceCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteNurseExperienceCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/experiences", new
        {
            facilityName = "City Hospital",
            jobTitle = "Registered Nurse",
            startDate = "2021-01-01",
            isCurrent = true,
            description = "ICU"
        });
        var listResponse = await _client.GetAsync("/api/v1/me/nurse-profile/experiences");
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/me/nurse-profile/experiences/{experienceId}", new
        {
            facilityName = "City Hospital",
            jobTitle = "Registered Nurse",
            startDate = "2021-01-01",
            isCurrent = true,
            description = "ICU"
        });
        var deleteResponse = await _client.DeleteAsync($"/api/v1/me/nurse-profile/experiences/{experienceId}");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var json = await listResponse.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<List<NurseExperienceDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(experienceId, Assert.Single(body).Id);
        _senderMock.Verify(s => s.Send(It.Is<UpdateNurseExperienceCommand>(c => c.Id == experienceId), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.Is<DeleteNurseExperienceCommand>(c => c.Id == experienceId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
