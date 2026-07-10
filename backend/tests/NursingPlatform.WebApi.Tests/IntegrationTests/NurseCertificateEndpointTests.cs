using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseCertificate;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseCertificateEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseCertificateEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListCertificates_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/certificates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Certificates_CreateListUpdateDelete_WorksForMetadataOnlyRecords()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var certificateId = Guid.NewGuid();
        var dto = new NurseCertificateDto
        {
            Id = certificateId,
            Name = "BLS",
            IssuingOrganization = "American Heart Association",
            IssueDate = new DateOnly(2024, 1, 1),
            ExpirationDate = new DateOnly(2026, 1, 1),
            CredentialId = "BLS-123"
        };
        _senderMock.Setup(s => s.Send(It.IsAny<CreateNurseCertificateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<ListCurrentNurseCertificatesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync([dto]);
        _senderMock.Setup(s => s.Send(It.IsAny<UpdateNurseCertificateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteNurseCertificateCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/certificates", new
        {
            name = "BLS",
            issuingOrganization = "American Heart Association",
            issueDate = "2024-01-01",
            expirationDate = "2026-01-01",
            credentialId = "BLS-123"
        });
        var listResponse = await _client.GetAsync("/api/v1/me/nurse-profile/certificates");
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/me/nurse-profile/certificates/{certificateId}", new
        {
            name = "BLS",
            issuingOrganization = "American Heart Association",
            issueDate = "2024-01-01",
            expirationDate = "2026-01-01",
            credentialId = "BLS-123"
        });
        var deleteResponse = await _client.DeleteAsync($"/api/v1/me/nurse-profile/certificates/{certificateId}");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var json = await listResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("storageKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<List<NurseCertificateDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(certificateId, Assert.Single(body).Id);
    }
}
