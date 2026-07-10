using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCv;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class DeleteNurseCvCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();

    [Fact]
    public async Task DeleteCv_ExistingCv_DeletesStoredFileAndMetadata()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var cvDocument = new NurseCvDocument
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            OriginalFileName = "cv.pdf",
            StorageKey = "nurse-cv/cv.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14,
            UploadedAt = DateTime.UtcNow
        };
        var cvDocuments = new List<NurseCvDocument> { cvDocument }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateNurseUser(userId), [profile], [cvDocument], cvDocuments);
        var handler = CreateHandler();

        await handler.Handle(new DeleteNurseCvCommand(), CancellationToken.None);

        _fileStorageMock.Verify(s => s.DeleteAsync("nurse-cv/cv.pdf", It.IsAny<CancellationToken>()), Times.Once);
        cvDocuments.Verify(d => d.Remove(cvDocument), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCv_NoCv_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        ConfigureContext(userId, CreateNurseUser(userId), [profile], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteNurseCvCommand(), CancellationToken.None));

        _fileStorageMock.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private DeleteNurseCvCommandHandler CreateHandler()
    {
        return new DeleteNurseCvCommandHandler(
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

    private static NurseProfile CreateProfile(Guid userId) => new() { Id = Guid.NewGuid(), UserId = userId };

    private static User CreateNurseUser(Guid userId)
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
                    Role = new Role { Id = roleId, Name = "Nurse" }
                }
            ]
        };
    }
}
