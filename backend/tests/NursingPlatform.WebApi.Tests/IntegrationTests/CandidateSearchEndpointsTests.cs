using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Application.Recruitment.Queries.ListCandidates;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class CandidateSearchEndpointsTests
{
    private static readonly string[] ForbiddenPropertyPatterns =
    [
        "\"userId\"",
        "\"email\"",
        "\"phone\"",
        "\"passwordHash\"",
        "\"roles\"",
        "\"permissions\"",
        "\"accessToken\"",
        "\"refreshToken\"",
        "\"tokenHash\"",
        "\"emailVerified\"",
        "\"isActive\"",
        "\"licenseNumber\"",
        "\"cvStorageKey\"",
        "\"cvFileUrl\"",
        "\"storageKey\"",
        "\"internalPath\"",
        "\"fileUrl\"",
        "\"firstName\"",
        "\"lastName\"",
        "\"displayLabel\"",
        "\"user\"",
        "\"nurseProfile\"",
        "\"licenseCountry\"",
        "\"currentCountry\"",
        "\"country\""
    ];

    private static readonly string[] ForbiddenSensitiveValues =
    [
        "hidden.candidate@example.com",
        "HiddenFirstName",
        "HiddenLastName",
        "hidden-license-number",
        "hidden-cv-storage-key",
        "hidden-password-hash"
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public CandidateSearchEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListCandidates_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/recruitment/candidates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListCandidates_WithAuthenticatedNonEmployer_ReturnsForbidden()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Employer role is required."));

        var response = await _client.GetAsync("/api/v1/recruitment/candidates");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData("Employer profile is required before searching candidates.")]
    [InlineData("Employer organization is required before searching candidates.")]
    public async Task ListCandidates_WhenEmployerPrerequisiteIsMissing_ReturnsForbidden(string message)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException(message));

        var response = await _client.GetAsync("/api/v1/recruitment/candidates");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ListCandidates_WithInvalidPagination_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListCandidatesQuery>(q => q.Page == 0), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Page", "Page must be at least 1.")]));

        var response = await _client.GetAsync("/api/v1/recruitment/candidates?page=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.True(body.GetProperty("errors").TryGetProperty("Page", out _));
    }

    [Fact]
    public async Task ListCandidates_WithEligibleEmployer_ReturnsPaginatedCandidateResponseWithoutSensitiveFields()
    {
        var nurseProfileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>
            {
                Items =
                [
                    new CandidateListItemDto
                    {
                        NurseProfileId = nurseProfileId,
                        Headline = "Critical care nurse",
                        ProfessionalSummary = "Experienced ICU nurse",
                        LicenseCountryName = "Canada",
                        CurrentCountryName = "United Kingdom",
                        YearsOfExperience = 8,
                        Skills = ["ICU", "Triage"],
                        Languages =
                        [
                            new CandidateLanguageDto
                            {
                                Name = "English",
                                Code = "en",
                                Proficiency = "Fluent"
                            }
                        ],
                        CertificatesSummary = "2 certificates",
                        CertificatesCount = 2,
                        LatestExperienceTitle = "Senior ICU Nurse",
                        EducationSummary = "Bachelor of Nursing"
                    }
                ],
                Page = 2,
                PageSize = 10,
                TotalCount = 11
            });

        var response = await _client.GetAsync("/api/v1/recruitment/candidates?page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeForbiddenCandidateFields(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("page").GetInt32());
        Assert.Equal(10, root.GetProperty("pageSize").GetInt32());
        Assert.Equal(11, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("items").ValueKind);
        Assert.Equal(1, root.GetProperty("items").GetArrayLength());

        var item = root.GetProperty("items")[0];
        Assert.Equal(nurseProfileId, item.GetProperty("nurseProfileId").GetGuid());
        Assert.Equal("Canada", item.GetProperty("licenseCountryName").GetString());
        Assert.Equal("United Kingdom", item.GetProperty("currentCountryName").GetString());

        var body = JsonSerializer.Deserialize<PaginatedResult<CandidateListItemDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
        Assert.Equal(10, body.PageSize);
        Assert.Equal(11, body.TotalCount);
        Assert.Equal(2, body.TotalPages);
        var candidate = Assert.Single(body.Items);
        Assert.Equal("Critical care nurse", candidate.Headline);
        Assert.Equal("ICU", candidate.Skills[0]);
        Assert.Equal("English", Assert.Single(candidate.Languages).Name);
    }

    [Fact]
    public async Task ListCandidates_WithFilteredEligibleEmployer_ReturnsPaginatedCandidateResponseWithoutSensitiveFields()
    {
        var nurseProfileId = Guid.NewGuid();
        var licenseCountryId = Guid.NewGuid();
        var currentCountryId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(
                It.Is<ListCandidatesQuery>(q =>
                    q.LicenseCountryId == licenseCountryId
                    && q.CurrentCountryId == currentCountryId
                    && q.MinimumYearsOfExperience == 5
                    && q.LanguageId == languageId
                    && q.Skills.SequenceEqual(new[] { "ICU", "Triage" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>
            {
                Items =
                [
                    new CandidateListItemDto
                    {
                        NurseProfileId = nurseProfileId,
                        Headline = "Filtered critical care nurse",
                        ProfessionalSummary = "Experienced ICU nurse",
                        LicenseCountryName = "Canada",
                        CurrentCountryName = "United Kingdom",
                        YearsOfExperience = 8,
                        Skills = ["ICU", "Triage"],
                        Languages =
                        [
                            new CandidateLanguageDto
                            {
                                Name = "English",
                                Code = "en",
                                Proficiency = "Fluent"
                            }
                        ],
                        CertificatesSummary = "2 certificates",
                        CertificatesCount = 2,
                        LatestExperienceTitle = "Senior ICU Nurse",
                        EducationSummary = "Bachelor of Nursing"
                    }
                ],
                Page = 1,
                PageSize = 20,
                TotalCount = 1
            });

        var response = await _client.GetAsync(
            $"/api/v1/recruitment/candidates?licenseCountryId={licenseCountryId}&currentCountryId={currentCountryId}&minimumYearsOfExperience=5&languageId={languageId}&skills=ICU&skills=Triage");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        AssertDoesNotExposeForbiddenCandidateFields(json);

        var body = JsonSerializer.Deserialize<PaginatedResult<CandidateListItemDto>>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        var candidate = Assert.Single(body.Items);
        Assert.Equal(nurseProfileId, candidate.NurseProfileId);
        Assert.Equal("Filtered critical care nurse", candidate.Headline);
        Assert.Equal(["ICU", "Triage"], candidate.Skills);
    }

    [Fact]
    public async Task ListCandidates_SendsQueryWithDefaultPagination()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        await _client.GetAsync("/api/v1/recruitment/candidates");

        _senderMock.Verify(s => s.Send(
            It.Is<ListCandidatesQuery>(q => q.Page == 1 && q.PageSize == 20),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCandidates_SendsQueryWithProvidedPagination()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        await _client.GetAsync("/api/v1/recruitment/candidates?page=2&pageSize=10");

        _senderMock.Verify(s => s.Send(
            It.Is<ListCandidatesQuery>(q => q.Page == 2 && q.PageSize == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCandidates_SendsQueryWithProvidedFilters()
    {
        var licenseCountryId = Guid.NewGuid();
        var currentCountryId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        var response = await _client.GetAsync(
            $"/api/v1/recruitment/candidates?page=3&pageSize=15&licenseCountryId={licenseCountryId}&currentCountryId={currentCountryId}&minimumYearsOfExperience=6&languageId={languageId}&skills=ICU");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _senderMock.Verify(s => s.Send(
            It.Is<ListCandidatesQuery>(q =>
                q.Page == 3
                && q.PageSize == 15
                && q.LicenseCountryId == licenseCountryId
                && q.CurrentCountryId == currentCountryId
                && q.MinimumYearsOfExperience == 6
                && q.LanguageId == languageId
                && q.Skills.SequenceEqual(new[] { "ICU" })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCandidates_SendsQueryWithRepeatedSkills()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        var response = await _client.GetAsync("/api/v1/recruitment/candidates?skills=ICU&skills=Triage");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _senderMock.Verify(s => s.Send(
            It.Is<ListCandidatesQuery>(q => q.Skills.SequenceEqual(new[] { "ICU", "Triage" })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCandidates_SendsQueryWithCommaSeparatedSkills()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        var response = await _client.GetAsync("/api/v1/recruitment/candidates?skills=ICU,Triage");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _senderMock.Verify(s => s.Send(
            It.Is<ListCandidatesQuery>(q => q.Skills.SequenceEqual(new[] { "ICU,Triage" })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCandidates_WithInvalidFilterValidation_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListCandidatesQuery>(q => q.MinimumYearsOfExperience == -1), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("MinimumYearsOfExperience", "Minimum years of experience must be at least 0.")]));

        var response = await _client.GetAsync("/api/v1/recruitment/candidates?minimumYearsOfExperience=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Validation failed", body.GetProperty("title").GetString());
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.True(body.GetProperty("errors").TryGetProperty("MinimumYearsOfExperience", out _));
    }

    [Theory]
    [InlineData("licenseCountryId")]
    [InlineData("currentCountryId")]
    [InlineData("languageId")]
    public async Task ListCandidates_WithInvalidGuidFilter_ReturnsBadRequest(string parameterName)
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.GetAsync($"/api/v1/recruitment/candidates?{parameterName}=not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListCandidates_UsesRequireAuthorizationOnly_WithoutPermissionSetup()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CandidateListItemDto>());

        var response = await _client.GetAsync("/api/v1/recruitment/candidates");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListCandidatesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void AssertDoesNotExposeForbiddenCandidateFields(string json)
    {
        foreach (var propertyPattern in ForbiddenPropertyPatterns)
        {
            Assert.DoesNotContain(propertyPattern, json, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var sensitiveValue in ForbiddenSensitiveValues)
        {
            Assert.DoesNotContain(sensitiveValue, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
