using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;
using NursingPlatform.Application.Employers.DTOs;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerOrganization;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class EmployerProfileEndpointsTests
{
    private static readonly string[] SensitiveFields =
    [
        "passwordHash",
        "accessToken",
        "refreshToken",
        "tokenHash",
        "roles",
        "permissions",
        "normalizedName",
        "storageKey",
        "internalPath",
        "fileUrl",
        "\"user\"",
        "\"employerProfile\"",
        "\"country\""
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public EmployerProfileEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployerProfile_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/employer-profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutEmployerProfile_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile", new
        {
            jobTitle = "Recruitment Manager"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployerOrganization_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/employer-profile/organization");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutEmployerOrganization_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile/organization", new
        {
            name = "General Hospital"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutEmployerProfile_WithAuthenticatedNonEmployer_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertMyEmployerProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Employer role is required."));

        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile", new
        {
            jobTitle = "Recruitment Manager"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task PutEmployerOrganization_WithAuthenticatedNonEmployer_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertMyEmployerOrganizationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Employer role is required."));

        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile/organization", new
        {
            name = "General Hospital"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task PutEmployerProfile_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertMyEmployerProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerProfileDto
            {
                Id = profileId,
                UserId = userId,
                JobTitle = "Recruitment Manager",
                Department = "Talent Acquisition"
            });

        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile", new
        {
            jobTitle = "Recruitment Manager",
            department = "Talent Acquisition"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeSensitiveFields(json);
        var body = JsonSerializer.Deserialize<EmployerProfileDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(profileId, body.Id);
        Assert.Equal(userId, body.UserId);
        Assert.Equal("Recruitment Manager", body.JobTitle);
        Assert.Equal("Talent Acquisition", body.Department);
        _senderMock.Verify(s => s.Send(
            It.Is<UpsertMyEmployerProfileCommand>(c =>
                c.JobTitle == "Recruitment Manager" &&
                c.Department == "Talent Acquisition"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployerProfile_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyEmployerProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerProfileDto
            {
                Id = profileId,
                UserId = userId,
                JobTitle = "Recruitment Manager",
                Department = "Talent Acquisition"
            });

        var response = await _client.GetAsync("/api/v1/me/employer-profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeSensitiveFields(json);
        var body = JsonSerializer.Deserialize<EmployerProfileDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(profileId, body.Id);
        Assert.Equal("Recruitment Manager", body.JobTitle);
    }

    [Fact]
    public async Task PutEmployerOrganization_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields()
    {
        var organizationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertMyEmployerOrganizationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerOrganizationDto
            {
                Id = organizationId,
                EmployerProfileId = profileId,
                Name = "General Hospital",
                Type = "Hospital",
                WebsiteUrl = "https://general.example.com",
                CountryId = countryId,
                CountryName = "Canada",
                City = "Toronto",
                AddressLine1 = "100 Care Street",
                AddressLine2 = "Suite 200",
                PostalCode = "A1B 2C3",
                Description = "Regional healthcare organization"
            });

        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile/organization", new
        {
            name = "General Hospital",
            type = "Hospital",
            websiteUrl = "https://general.example.com",
            countryId,
            city = "Toronto",
            addressLine1 = "100 Care Street",
            addressLine2 = "Suite 200",
            postalCode = "A1B 2C3",
            description = "Regional healthcare organization"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeSensitiveFields(json);
        var body = JsonSerializer.Deserialize<EmployerOrganizationDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(organizationId, body.Id);
        Assert.Equal(profileId, body.EmployerProfileId);
        Assert.Equal("General Hospital", body.Name);
        Assert.Equal("Canada", body.CountryName);
        _senderMock.Verify(s => s.Send(
            It.Is<UpsertMyEmployerOrganizationCommand>(c =>
                c.Name == "General Hospital" &&
                c.Type == "Hospital" &&
                c.WebsiteUrl == "https://general.example.com" &&
                c.CountryId == countryId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployerOrganization_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields()
    {
        var organizationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyEmployerOrganizationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerOrganizationDto
            {
                Id = organizationId,
                EmployerProfileId = profileId,
                Name = "General Hospital",
                Type = "Hospital",
                WebsiteUrl = "https://general.example.com",
                City = "Toronto"
            });

        var response = await _client.GetAsync("/api/v1/me/employer-profile/organization");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeSensitiveFields(json);
        var body = JsonSerializer.Deserialize<EmployerOrganizationDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(organizationId, body.Id);
        Assert.Equal("General Hospital", body.Name);
    }

    [Fact]
    public async Task PutEmployerOrganization_WithInvalidPayload_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertMyEmployerOrganizationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Name", "Name is required.")]));

        var response = await _client.PutAsJsonAsync("/api/v1/me/employer-profile/organization", new
        {
            name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Name", out _));
    }

    private static void AssertDoesNotExposeSensitiveFields(string json)
    {
        foreach (var field in SensitiveFields)
        {
            Assert.DoesNotContain(field, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
