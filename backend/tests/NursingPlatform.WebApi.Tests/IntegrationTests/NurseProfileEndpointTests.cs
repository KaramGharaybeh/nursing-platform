using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseProfileEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseProfileEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutProfile_WithNurseJwt_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _senderMock
            .Setup(s => s.Send(It.IsAny<UpsertNurseProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NurseProfileDto
            {
                Id = profileId,
                UserId = userId,
                Headline = "Registered Nurse",
                ProfessionalSummary = "ICU nurse",
                LicenseNumber = "RN-123",
                YearsOfExperience = 5,
                IsAvailableForRecruitment = true
            });

        var response = await _client.PutAsJsonAsync("/api/v1/me/nurse-profile", new
        {
            headline = "Registered Nurse",
            professionalSummary = "ICU nurse",
            licenseNumber = "RN-123",
            yearsOfExperience = 5,
            isAvailableForRecruitment = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("permissions", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<NurseProfileDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(profileId, body.Id);
        Assert.Equal("Registered Nurse", body.Headline);
        _senderMock.Verify(s => s.Send(
            It.Is<UpsertNurseProfileCommand>(c =>
                c.Headline == "Registered Nurse" &&
                c.ProfessionalSummary == "ICU nurse" &&
                c.LicenseNumber == "RN-123" &&
                c.YearsOfExperience == 5 &&
                c.IsAvailableForRecruitment),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_WithAuthenticatedNonNurse_ReturnsForbiddenProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetCurrentNurseProfileQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAccessException("Nurse role is required."));

        var response = await _client.GetAsync("/api/v1/me/nurse-profile");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Forbidden", body.GetProperty("title").GetString());
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }
}

internal static class NurseEndpointTestAuth
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void Authorize(HttpClient client, Guid userId)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId));
    }

    private static string CreateJwt(Guid userId)
    {
        const string secret = "test-secret-key-that-is-at-least-32-characters-long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        {
            KeyId = "nursing-platform-key"
        };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
