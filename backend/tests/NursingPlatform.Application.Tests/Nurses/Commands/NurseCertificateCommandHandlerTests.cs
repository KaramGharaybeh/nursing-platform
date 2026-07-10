using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseCertificate;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class NurseCertificateCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task CreateCertificate_ValidRequest_AddsMetadataOnlyRecord()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var certificateSet = new List<NurseCertificate>().AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], certificateSet);
        var handler = CreateCreateHandler();

        var command = new CreateNurseCertificateCommand
        {
            Name = "Critical Care Certificate",
            IssuingOrganization = "Nursing Board",
            IssueDate = new DateOnly(2023, 1, 1),
            ExpirationDate = new DateOnly(2026, 1, 1),
            CredentialId = "CERT-123",
            CredentialUrl = "https://credentials.example/cert-123"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.IssuingOrganization, result.IssuingOrganization);
        Assert.Equal(command.CredentialUrl, result.CredentialUrl);
        Assert.Null(typeof(NursingPlatform.Application.Nurses.DTOs.NurseCertificateDto).GetProperty("StorageKey"));
        Assert.Null(typeof(NursingPlatform.Application.Nurses.DTOs.NurseCertificateDto).GetProperty("FilePath"));
        certificateSet.Verify(c => c.Add(It.Is<NurseCertificate>(x =>
            x.NurseProfileId == profile.Id &&
            x.Name == command.Name &&
            x.IssuingOrganization == command.IssuingOrganization &&
            x.IssueDate == command.IssueDate &&
            x.ExpirationDate == command.ExpirationDate &&
            x.CredentialId == command.CredentialId &&
            x.CredentialUrl == command.CredentialUrl)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCertificate_RecordOwnedByAnotherProfile_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var currentProfile = CreateProfile(userId);
        var otherProfile = CreateProfile(Guid.NewGuid());
        var certificate = CreateCertificate(otherProfile.Id, new DateOnly(2022, 1, 1), DateTime.UtcNow);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [currentProfile, otherProfile], [certificate]);
        var handler = CreateUpdateHandler();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateNurseCertificateCommand
            {
                Id = certificate.Id,
                Name = "Updated Certificate",
                IssuingOrganization = "Updated Board"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCertificate_RecordOwnedByCurrentProfile_RemovesRecord()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var certificate = CreateCertificate(profile.Id, new DateOnly(2022, 1, 1), DateTime.UtcNow);
        var certificateSet = new List<NurseCertificate> { certificate }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [certificate], certificateSet);
        var handler = CreateDeleteHandler();

        await handler.Handle(new DeleteNurseCertificateCommand(certificate.Id), CancellationToken.None);

        certificateSet.Verify(c => c.Remove(certificate), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Certificate_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], []);
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateNurseCertificateCommand
            {
                Name = "Critical Care Certificate",
                IssuingOrganization = "Nursing Board"
            }, CancellationToken.None));
    }

    private CreateNurseCertificateCommandHandler CreateCreateHandler()
    {
        return new CreateNurseCertificateCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private UpdateNurseCertificateCommandHandler CreateUpdateHandler()
    {
        return new UpdateNurseCertificateCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private DeleteNurseCertificateCommandHandler CreateDeleteHandler()
    {
        return new DeleteNurseCertificateCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseCertificate> certificates,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseCertificate>>? certificateMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseCertificates).Returns(certificateMock?.Object ?? certificates.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Countries).Returns(Array.Empty<Country>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static NurseProfile CreateProfile(Guid userId)
    {
        return new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
    }

    private static NurseCertificate CreateCertificate(Guid profileId, DateOnly? issueDate, DateTime createdAt)
    {
        return new NurseCertificate
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            Name = "Critical Care Certificate",
            IssuingOrganization = "Nursing Board",
            IssueDate = issueDate,
            CreatedAt = createdAt
        };
    }

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
