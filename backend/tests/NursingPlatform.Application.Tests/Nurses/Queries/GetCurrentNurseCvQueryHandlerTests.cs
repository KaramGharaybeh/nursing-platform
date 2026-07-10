using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class GetCurrentNurseCvQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task GetCv_ExistingCv_ReturnsMetadataOnly()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var uploadedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var cvDocument = new NurseCvDocument
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            OriginalFileName = "cv.pdf",
            StorageKey = "nurse-cv/internal.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14,
            UploadedAt = uploadedAt
        };
        ConfigureContext(userId, CreateNurseUser(userId), [profile], [cvDocument]);
        var handler = CreateHandler();

        var result = await handler.Handle(new GetCurrentNurseCvQuery(), CancellationToken.None);

        Assert.Equal(cvDocument.Id, result.Id);
        Assert.Equal("cv.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(14, result.FileSizeBytes);
        Assert.Equal(uploadedAt, result.UploadedAt);
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("StorageKey"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("StorageRoot"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("InternalPath"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("FileUrl"));
        Assert.Null(typeof(NurseCvDocumentDto).GetProperty("NurseProfile"));
    }

    [Fact]
    public async Task GetCv_NoCv_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        ConfigureContext(userId, CreateNurseUser(userId), [profile], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetCurrentNurseCvQuery(), CancellationToken.None));
    }

    private GetCurrentNurseCvQueryHandler CreateHandler()
    {
        return new GetCurrentNurseCvQueryHandler(
            _contextMock.Object,
            new NursingPlatform.Application.Nurses.Common.NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseCvDocument> cvDocuments)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseCvDocuments).Returns(cvDocuments.AsQueryable().BuildMockDbSet().Object);
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
