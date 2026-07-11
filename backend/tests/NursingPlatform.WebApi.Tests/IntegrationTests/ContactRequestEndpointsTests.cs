using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;
using NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;
using NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;
using NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class ContactRequestEndpointsTests
{
    private static readonly string[] ForbiddenPropertyPatterns =
    [
        "\"userId\"",
        "\"employerProfileId\"",
        "\"employerOrganizationId\"",
        "\"email\"",
        "\"phone\"",
        "\"passwordHash\"",
        "\"roles\"",
        "\"permissions\"",
        "\"accessToken\"",
        "\"refreshToken\"",
        "\"tokenHash\"",
        "\"licenseNumber\"",
        "\"cvStorageKey\"",
        "\"cvFileUrl\"",
        "\"storageKey\"",
        "\"internalPath\"",
        "\"fileUrl\"",
        "\"firstName\"",
        "\"lastName\"",
        "\"user\"",
        "\"nurseProfile\"",
        "\"employerProfile\"",
        "\"employerOrganization\"",
        "\"message\"",
        "\"rejectionReason\"",
        "\"rowVersion\"",
        "\"concurrencyToken\""
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public ContactRequestEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("POST", "/api/v1/recruitment/contact-requests")]
    [InlineData("GET", "/api/v1/recruitment/contact-requests")]
    [InlineData("GET", "/api/v1/recruitment/contact-requests/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/v1/recruitment/contact-requests/11111111-1111-1111-1111-111111111111/cancel")]
    [InlineData("GET", "/api/v1/me/nurse-profile/contact-requests")]
    [InlineData("POST", "/api/v1/me/nurse-profile/contact-requests/11111111-1111-1111-1111-111111111111/approve")]
    [InlineData("POST", "/api/v1/me/nurse-profile/contact-requests/11111111-1111-1111-1111-111111111111/reject")]
    public async Task ContactRequestEndpoints_WithoutJwt_ReturnUnauthorized(string method, string path)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method == "POST"
                ? new StringContent("{}", Encoding.UTF8, "application/json")
                : null
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateContactRequest_WithInvalidBody_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("NurseProfileId", "Nurse profile id is required.")]));

        var response = await _client.PostAsJsonAsync("/api/v1/recruitment/contact-requests", new { nurseProfileId = Guid.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("NurseProfileId", out _));
    }

    [Fact]
    public async Task ListMyContactRequests_WithInvalidStatus_ReturnsBadRequest()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.GetAsync("/api/v1/recruitment/contact-requests?status=Unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListMyContactRequestsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateContactRequest_WithForbiddenPrerequisite_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Employer profile is required before requesting contact."));

        var response = await _client.PostAsJsonAsync("/api/v1/recruitment/contact-requests", new { nurseProfileId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyContactRequest_WhenHidden_ReturnsNotFound()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyContactRequestQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Contact request was not found."));

        var response = await _client.GetAsync($"/api/v1/recruitment/contact-requests/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateContactRequest_WithDuplicate_ReturnsConflict()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("An active contact request already exists for this candidate."));

        var response = await _client.PostAsJsonAsync("/api/v1/recruitment/contact-requests", new { nurseProfileId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ApproveReceivedContactRequest_WithInvalidTransition_ReturnsConflict()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ApproveReceivedContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Only pending contact requests can be approved."));

        var response = await _client.PostAsync($"/api/v1/me/nurse-profile/contact-requests/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContactRequest_WithValidRequest_ReturnsCreatedAndSafeJson()
    {
        var requestId = Guid.NewGuid();
        var nurseProfileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<CreateContactRequestCommand>(c => c.NurseProfileId == nurseProfileId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEmployerDto(requestId, nurseProfileId));

        var response = await _client.PostAsJsonAsync("/api/v1/recruitment/contact-requests", new { nurseProfileId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/recruitment/contact-requests/{requestId}", response.Headers.Location?.ToString());
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeForbiddenFields(json);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(nurseProfileId, document.RootElement.GetProperty("nurseProfileId").GetGuid());
        Assert.Equal("Pending", document.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ListMyContactRequests_ReturnsPaginatedSafeJson()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyContactRequestsQuery>(q => q.Page == 2 && q.PageSize == 5 && q.Status == ContactRequestStatus.Pending), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ContactRequestDto>
            {
                Items = [CreateEmployerDto(Guid.NewGuid(), Guid.NewGuid())],
                Page = 2,
                PageSize = 5,
                TotalCount = 6
            });

        var response = await _client.GetAsync("/api/v1/recruitment/contact-requests?page=2&pageSize=5&status=Pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeForbiddenFields(json);
        var body = JsonSerializer.Deserialize<PaginatedResult<ContactRequestDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
        Assert.Equal(2, body.TotalPages);
    }

    [Fact]
    public async Task ListReceivedContactRequests_ReturnsPaginatedSafeJson()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListReceivedContactRequestsQuery>(q => q.Page == 1 && q.PageSize == 20), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ReceivedContactRequestDto>
            {
                Items =
                [
                    new ReceivedContactRequestDto
                    {
                        Id = Guid.NewGuid(),
                        OrganizationName = "General Hospital",
                        JobTitle = "Recruitment Manager",
                        Department = "Clinical Hiring",
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ],
                Page = 1,
                PageSize = 20,
                TotalCount = 1
            });

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/contact-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeForbiddenFields(json);
        Assert.Contains("General Hospital", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ContactRequestEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListMyContactRequestsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ContactRequestDto>());

        var response = await _client.GetAsync("/api/v1/recruitment/contact-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListMyContactRequestsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransitionEndpoints_SendExpectedCommands()
    {
        var id = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock.Setup(s => s.Send(It.IsAny<CancelContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEmployerDto(id, Guid.NewGuid()));
        _senderMock.Setup(s => s.Send(It.IsAny<RejectReceivedContactRequestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceivedContactRequestDto { Id = id, OrganizationName = "General Hospital", Status = "Rejected" });

        var cancelResponse = await _client.PostAsync($"/api/v1/recruitment/contact-requests/{id}/cancel", null);
        var rejectResponse = await _client.PostAsync($"/api/v1/me/nurse-profile/contact-requests/{id}/reject", null);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);
        _senderMock.Verify(s => s.Send(It.Is<CancelContactRequestCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.Is<RejectReceivedContactRequestCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ContactRequestDto CreateEmployerDto(Guid id, Guid nurseProfileId)
    {
        return new ContactRequestDto
        {
            Id = id,
            NurseProfileId = nurseProfileId,
            Status = "Pending",
            CandidateHeadline = "ICU nurse",
            CandidateLicenseCountryName = "Canada",
            CandidateCurrentCountryName = "United Kingdom",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static void AssertDoesNotExposeForbiddenFields(string json)
    {
        foreach (var pattern in ForbiddenPropertyPatterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
