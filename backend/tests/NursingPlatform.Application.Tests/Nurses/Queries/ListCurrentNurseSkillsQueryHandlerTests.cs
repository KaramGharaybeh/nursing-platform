using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class ListCurrentNurseSkillsQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListSkills_ReturnsNamesSortedAscending()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var triage = new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = profile.Id, Name = "Triage", NormalizedName = "TRIAGE" };
        var criticalCare = new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = profile.Id, Name = "Critical Care", NormalizedName = "CRITICAL CARE" };
        var otherProfileSkill = new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = Guid.NewGuid(), Name = "Other", NormalizedName = "OTHER" };
        ConfigureContext(userId, CreateNurseUser(userId), [profile], [triage, otherProfileSkill, criticalCare]);
        var handler = new ListCurrentNurseSkillsQueryHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));

        var result = await handler.Handle(new ListCurrentNurseSkillsQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal("Critical Care", first.Name),
            second => Assert.Equal("Triage", second.Name));
        Assert.All(result, item => Assert.IsType<NurseSkillDto>(item));
        Assert.DoesNotContain(result, item => item.Name == "Other");
        Assert.Null(typeof(NurseSkillDto).GetProperty("NormalizedName"));
        Assert.Null(typeof(NurseSkillDto).GetProperty("NurseProfile"));
    }

    private void ConfigureContext(
        Guid currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseSkill> skills)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseSkills).Returns(skills.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Languages).Returns(Array.Empty<Language>().AsQueryable().BuildMockDbSet().Object);
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
