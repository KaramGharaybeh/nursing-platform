using System.Net;
using System.Text.Json;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseProfileOpenApiContractTests
{
    private readonly HttpClient _client;

    public NurseProfileOpenApiContractTests(WebApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseProfileOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertRefResponse(document, "/api/v1/me/nurse-profile", "get", "NurseProfileDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile", "put", "NurseProfileDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseExperienceOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/experiences", "get", "NurseExperienceDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/experiences", "post", "NurseExperienceDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/experiences/{id}", "put", "NurseExperienceDto");
        AssertNoContentResponse(document, "/api/v1/me/nurse-profile/experiences/{id}", "delete");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseEducationOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/education", "get", "NurseEducationDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/education", "post", "NurseEducationDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/education/{id}", "put", "NurseEducationDto");
        AssertNoContentResponse(document, "/api/v1/me/nurse-profile/education/{id}", "delete");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseCertificateOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/certificates", "get", "NurseCertificateDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/certificates", "post", "NurseCertificateDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/certificates/{id}", "put", "NurseCertificateDto");
        AssertNoContentResponse(document, "/api/v1/me/nurse-profile/certificates/{id}", "delete");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseLanguageOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/languages", "get", "NurseLanguageDto");
        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/languages", "put", "NurseLanguageDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseSkillOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/skills", "get", "NurseSkillDto");
        AssertArrayRefResponse(document, "/api/v1/me/nurse-profile/skills", "put", "NurseSkillDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForNurseCvOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertRefResponse(document, "/api/v1/me/nurse-profile/cv", "get", "NurseCvDocumentDto");
        AssertRefResponse(document, "/api/v1/me/nurse-profile/cv", "post", "NurseCvDocumentDto");
        AssertNoContentResponse(document, "/api/v1/me/nurse-profile/cv", "delete");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForReceivedContactRequestOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertRefResponse(
            document,
            "/api/v1/me/nurse-profile/contact-requests",
            "get",
            "PaginatedResultOfReceivedContactRequestDto");
        AssertRefResponse(
            document,
            "/api/v1/me/nurse-profile/contact-requests/{id}/approve",
            "post",
            "ReceivedContactRequestDto");
        AssertRefResponse(
            document,
            "/api/v1/me/nurse-profile/contact-requests/{id}/reject",
            "post",
            "ReceivedContactRequestDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForRecruitmentCandidateSearch()
    {
        using var document = await GetOpenApiDocument();

        AssertRefResponse(
            document,
            "/api/v1/recruitment/candidates",
            "get",
            "PaginatedResultOfCandidateListItemDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_PublishesTypedResponsesForEmployerContactRequestOperations()
    {
        using var document = await GetOpenApiDocument();

        AssertRefResponse(
            document,
            "/api/v1/recruitment/contact-requests",
            "post",
            "ContactRequestDto",
            "201");
        AssertRefResponse(
            document,
            "/api/v1/recruitment/contact-requests",
            "get",
            "PaginatedResultOfContactRequestDto");
        AssertRefResponse(
            document,
            "/api/v1/recruitment/contact-requests/{id}",
            "get",
            "ContactRequestDto");
        AssertRefResponse(
            document,
            "/api/v1/recruitment/contact-requests/{id}/cancel",
            "post",
            "ContactRequestDto");
    }

    [Fact]
    public async Task DevelopmentOpenApi_CandidateListItemDto_ExposesOnlyRecruitmentSafeFields()
    {
        using var document = await GetOpenApiDocument();

        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("CandidateListItemDto", out var candidateDto));

        var properties = candidateDto.GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var allowed in new[]
        {
            "nurseProfileId",
            "headline",
            "professionalSummary",
            "licenseCountryName",
            "currentCountryName",
            "yearsOfExperience",
            "skills",
            "languages",
            "certificatesSummary",
            "certificatesCount",
            "latestExperienceTitle",
            "educationSummary"
        })
        {
            Assert.Contains(allowed, properties);
        }

        foreach (var forbidden in new[]
        {
            "userId",
            "email",
            "username",
            "licenseNumber",
            "licenseCountryId",
            "currentCountryId",
            "cv",
            "cvDocument",
            "storageKey",
            "isAvailableForRecruitment"
        })
        {
            Assert.DoesNotContain(forbidden, properties);
        }
    }

    [Fact]
    public async Task DevelopmentOpenApi_NurseCvDocumentDto_ExposesMetadataOnlyFields()
    {
        using var document = await GetOpenApiDocument();

        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("NurseCvDocumentDto", out var cvDto));

        var properties = cvDto.GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var allowed in new[] { "id", "fileName", "contentType", "fileSizeBytes", "uploadedAt" })
        {
            Assert.Contains(allowed, properties);
        }

        foreach (var forbidden in new[] { "storageKey", "internalPath", "rootPath", "fileUrl", "content" })
        {
            Assert.DoesNotContain(forbidden, properties);
        }
    }

    private async Task<JsonDocument> GetOpenApiDocument()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static void AssertRefResponse(
        JsonDocument document,
        string path,
        string method,
        string expectedSchema,
        string statusCode = "200")
    {
        var response = GetOperation(document, path, method)
            .GetProperty("responses")
            .GetProperty(statusCode);
        var schema = response.GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");

        Assert.Equal($"#/components/schemas/{expectedSchema}", schema.GetProperty("$ref").GetString());
    }

    private static void AssertArrayRefResponse(
        JsonDocument document,
        string path,
        string method,
        string expectedItemSchema,
        string statusCode = "200")
    {
        var schema = GetOperation(document, path, method)
            .GetProperty("responses")
            .GetProperty(statusCode)
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");

        Assert.Equal("array", schema.GetProperty("type").GetString());
        Assert.Equal(
            $"#/components/schemas/{expectedItemSchema}",
            schema.GetProperty("items").GetProperty("$ref").GetString());
    }

    private static void AssertNoContentResponse(JsonDocument document, string path, string method)
    {
        var responses = GetOperation(document, path, method).GetProperty("responses");

        Assert.True(responses.TryGetProperty("204", out var noContent));
        Assert.Empty(noContent.EnumerateObject().Where(property => property.NameEquals("content")));
        Assert.False(responses.TryGetProperty("200", out _));
    }

    private static JsonElement GetOperation(JsonDocument document, string path, string method)
    {
        return document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method);
    }
}
