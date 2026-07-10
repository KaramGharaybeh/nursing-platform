using System.Text.Json;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.UploadNurseCv;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UploadNurseCvCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();

    [Fact]
    public async Task Upload_ValidPdf_StoresFileAndPersistsMetadata()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var cvDocuments = new List<NurseCvDocument>().AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], cvDocuments);
        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), ".pdf", "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFileResult
            {
                StorageKey = "nurse-cv/generated.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 14
            });
        var handler = CreateHandler();

        var result = await handler.Handle(new UploadNurseCvCommand
        {
            File = CreateStream(),
            OriginalFileName = "../cv.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14
        }, CancellationToken.None);

        _fileStorageMock.Verify(s => s.SaveAsync(It.IsAny<Stream>(), ".pdf", "application/pdf", It.IsAny<CancellationToken>()), Times.Once);
        cvDocuments.Verify(d => d.Add(It.Is<NurseCvDocument>(x =>
            x.NurseProfileId == profile.Id &&
            x.OriginalFileName == "cv.pdf" &&
            x.StorageKey == "nurse-cv/generated.pdf" &&
            x.ContentType == "application/pdf" &&
            x.FileSizeBytes == 14 &&
            x.UploadedAt.Kind == DateTimeKind.Utc)), Times.Once);
        Assert.Equal("cv.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(14, result.FileSizeBytes);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upload_ReplacesExistingCv_DeletesOldStoredFile()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var existingCv = new NurseCvDocument
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            OriginalFileName = "old.pdf",
            StorageKey = "nurse-cv/old.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 10,
            UploadedAt = DateTime.UtcNow.AddDays(-1)
        };
        var cvDocuments = new List<NurseCvDocument> { existingCv }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [existingCv], cvDocuments);
        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFileResult
            {
                StorageKey = "nurse-cv/new.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileSizeBytes = 14
            });
        var handler = CreateHandler();

        await handler.Handle(new UploadNurseCvCommand
        {
            File = CreateStream(),
            OriginalFileName = "new.docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSizeBytes = 14
        }, CancellationToken.None);

        cvDocuments.Verify(d => d.Remove(existingCv), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(s => s.DeleteAsync("nurse-cv/old.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(s => s.DeleteAsync("nurse-cv/new.docx", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Upload_ResponseDoesNotExposeStorageKey()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], []);
        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), ".pdf", "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFileResult
            {
                StorageKey = "internal-storage-key.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 14
            });
        var handler = CreateHandler();

        var result = await handler.Handle(new UploadNurseCvCommand
        {
            File = CreateStream(),
            OriginalFileName = "cv.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14
        }, CancellationToken.None);

        var json = JsonSerializer.Serialize(result);

        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("StorageKey"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("StorageRoot"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("InternalPath"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("FileUrl"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("NurseProfile"));
        Assert.DoesNotContain("storageKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal-storage-key", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_WhenSaveChangesFails_BestEffortDeletesNewStoredFileAndRethrows()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var failure = new InvalidOperationException("Database save failed.");
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], []);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(failure);
        _fileStorageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), ".pdf", "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFileResult
            {
                StorageKey = "nurse-cv/new.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 14
            });
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UploadNurseCvCommand
            {
                File = CreateStream(),
                OriginalFileName = "cv.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 14
            }, CancellationToken.None));

        Assert.Same(failure, exception);
        _fileStorageMock.Verify(s => s.DeleteAsync("nurse-cv/new.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cv_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UploadNurseCvCommand
            {
                File = CreateStream(),
                OriginalFileName = "cv.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 14
            }, CancellationToken.None));

        _fileStorageMock.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private UploadNurseCvCommandHandler CreateHandler()
    {
        return new UploadNurseCvCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object),
            _fileStorageMock.Object);
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseCvDocument> cvDocuments,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseCvDocument>>? cvDocumentMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseCvDocuments).Returns(cvDocumentMock?.Object ?? cvDocuments.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static MemoryStream CreateStream() => new("test cv content"u8.ToArray());

    private static NurseProfile CreateProfile(Guid userId) => new() { Id = Guid.NewGuid(), UserId = userId };

    private static User CreateUserWithRole(Guid userId, string roleName)
    {
        var roleId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            IsActive = true,
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role { Id = roleId, Name = roleName }
                }
            ]
        };
    }
}
