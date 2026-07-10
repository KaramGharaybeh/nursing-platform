using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCv;
using NursingPlatform.Application.Nurses.Commands.UploadNurseCv;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class NurseCvEndpointTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;

    public NurseCvEndpointTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _senderMock.Reset();
        factory.PermissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCv_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/me/nurse-profile/cv");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadCv_WithUnsupportedFileType_ReturnsBadRequest()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadNurseCvCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("ContentType", "CV content type is not supported.")]));

        var response = await _client.PostAsync("/api/v1/me/nurse-profile/cv", CreateMultipart("cv.txt", "text/plain", "not supported"u8.ToArray()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadCv_WithOversizedFile_ReturnsBadRequest()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadNurseCvCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("FileSizeBytes", "CV file must be 5 MB or smaller.")]));

        var response = await _client.PostAsync("/api/v1/me/nurse-profile/cv", CreateMultipart("cv.pdf", "application/pdf", new byte[(5 * 1024 * 1024) + 1]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cv_UploadGetAndDelete_ReturnsMetadataOnlyAndDeletesOwnedRecord()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var cvId = Guid.NewGuid();
        var dto = new NurseCvDocumentDto
        {
            Id = cvId,
            FileName = "cv.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14,
            UploadedAt = new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc)
        };
        _senderMock.Setup(s => s.Send(It.IsAny<UploadNurseCvCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<GetCurrentNurseCvQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _senderMock.Setup(s => s.Send(It.IsAny<DeleteNurseCvCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var uploadResponse = await _client.PostAsync("/api/v1/me/nurse-profile/cv", CreateMultipart("cv.pdf", "application/pdf", "test cv content"u8.ToArray()));
        var getResponse = await _client.GetAsync("/api/v1/me/nurse-profile/cv");
        var deleteResponse = await _client.DeleteAsync("/api/v1/me/nurse-profile/cv");

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var json = await uploadResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("storageKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internalPath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rootPath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fileUrl", json, StringComparison.OrdinalIgnoreCase);
        var body = JsonSerializer.Deserialize<NurseCvDocumentDto>(json, NurseEndpointTestAuth.JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(cvId, body.Id);
        _senderMock.Verify(s => s.Send(It.Is<UploadNurseCvCommand>(c =>
            c.OriginalFileName == "cv.pdf" &&
            c.ContentType == "application/pdf" &&
            c.FileSizeBytes == 15), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static MultipartFormDataContent CreateMultipart(string fileName, string contentType, byte[] content)
    {
        var multipart = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        multipart.Add(file, "file", fileName);
        return multipart;
    }
}
