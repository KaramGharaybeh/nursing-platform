using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseEducation;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseEducation;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseEducationEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseEducationEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListEducation_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/education");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Education_CreateListUpdateDelete_WorksForOwnedRecords()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var educationId = Guid.NewGuid();
        var dto = new NurseEducationDto
        {
            Id = educationId,
            InstitutionName = "Nursing College",
            Degree = "BSN",
            FieldOfStudy = "Nursing",
            StartDate = new DateOnly(2018, 1, 1),
            EndDate = new DateOnly(2021, 1, 1)
        };
        _senderMock.Setup(s => s.Send(It.IsAny<CreateNurseEducationCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<ListCurrentNurseEducationQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);
        _senderMock.Setup(s => s.Send(It.IsAny<UpdateNurseEducationCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteNurseEducationCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/education", new
        {
            institutionName = "Nursing College",
            degree = "BSN",
            fieldOfStudy = "Nursing",
            startDate = "2018-01-01",
            endDate = "2021-01-01"
        });
        var listResponse = await _client.GetAsync("/api/v1/me/nurse-profile/education");
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/me/nurse-profile/education/{educationId}", new
        {
            institutionName = "Nursing College",
            degree = "BSN",
            fieldOfStudy = "Nursing",
            startDate = "2018-01-01",
            endDate = "2021-01-01"
        });
        var deleteResponse = await _client.DeleteAsync($"/api/v1/me/nurse-profile/education/{educationId}");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var json = await listResponse.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<List<NurseEducationDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(educationId, Assert.Single(body).Id);
        _senderMock.Verify(s => s.Send(It.Is<UpdateNurseEducationCommand>(c => c.Id == educationId), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.Is<DeleteNurseEducationCommand>(c => c.Id == educationId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
