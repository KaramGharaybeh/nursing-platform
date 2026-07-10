using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class ListCurrentNurseCertificatesQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListCertificates_ReturnsRecordsSortedByIssueDateDescendingThenCreatedAtDescending()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var oldest = CreateCertificate(profile.Id, "Old Certificate", new DateOnly(2020, 1, 1), new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newest = CreateCertificate(profile.Id, "Newest Certificate", new DateOnly(2024, 1, 1), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var tieBreakerNewest = CreateCertificate(profile.Id, "Tie Newest Certificate", new DateOnly(2024, 1, 1), new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        var otherProfileCertificate = CreateCertificate(Guid.NewGuid(), "Other Certificate", new DateOnly(2025, 1, 1), DateTime.UtcNow);
        ConfigureContext(
            userId,
            CreateNurseUser(userId),
            [profile],
            [oldest, newest, tieBreakerNewest, otherProfileCertificate]);
        var handler = new ListCurrentNurseCertificatesQueryHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));

        var result = await handler.Handle(new ListCurrentNurseCertificatesQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal(tieBreakerNewest.Id, first.Id),
            second => Assert.Equal(newest.Id, second.Id),
            third => Assert.Equal(oldest.Id, third.Id));
        Assert.All(result, item => Assert.IsType<NurseCertificateDto>(item));
        Assert.DoesNotContain(result, item => item.Id == otherProfileCertificate.Id);
        Assert.Null(typeof(NurseCertificateDto).GetProperty("NurseProfile"));
        Assert.Null(typeof(NurseCertificateDto).GetProperty("StorageKey"));
        Assert.Null(typeof(NurseCertificateDto).GetProperty("FilePath"));
    }

    private void ConfigureContext(
        Guid currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseCertificate> certificates)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseCertificates).Returns(certificates.AsQueryable().BuildMockDbSet().Object);
    }

    private static NurseCertificate CreateCertificate(
        Guid profileId,
        string name,
        DateOnly? issueDate,
        DateTime createdAt)
    {
        return new NurseCertificate
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profileId,
            Name = name,
            IssuingOrganization = "Nursing Board",
            IssueDate = issueDate,
            CreatedAt = createdAt
        };
    }

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
