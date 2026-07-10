using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpdateNurseSkillsCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task UpdateSkills_ValidRequest_ReplacesExistingSkills()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var oldSkill = new NurseSkill { Id = Guid.NewGuid(), NurseProfileId = profile.Id, Name = "Old Skill", NormalizedName = "OLD SKILL" };
        var nurseSkills = new List<NurseSkill> { oldSkill }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [oldSkill], nurseSkills);
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateNurseSkillsCommand
        {
            Skills = [" Critical   Care ", "Triage"]
        }, CancellationToken.None);

        nurseSkills.Verify(s => s.Remove(oldSkill), Times.Once);
        nurseSkills.Verify(s => s.Add(It.Is<NurseSkill>(x =>
            x.NurseProfileId == profile.Id && x.Name == "Critical Care" && x.NormalizedName == "CRITICAL CARE")), Times.Once);
        nurseSkills.Verify(s => s.Add(It.Is<NurseSkill>(x =>
            x.NurseProfileId == profile.Id && x.Name == "Triage" && x.NormalizedName == "TRIAGE")), Times.Once);
        Assert.Contains(result, item => item.Name == "Critical Care");
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSkills_StoresNormalizedNamesAndReturnsDisplayNamesOnly()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var nurseSkills = new List<NurseSkill>().AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], nurseSkills);
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateNurseSkillsCommand
        {
            Skills = ["  wound   care  "]
        }, CancellationToken.None);

        nurseSkills.Verify(s => s.Add(It.Is<NurseSkill>(x => x.Name == "wound care" && x.NormalizedName == "WOUND CARE")), Times.Once);
        Assert.Single(result);
        Assert.Equal("wound care", result[0].Name);
        Assert.Null(typeof(NursingPlatform.Application.Nurses.DTOs.NurseSkillDto).GetProperty("NormalizedName"));
    }

    [Fact]
    public async Task Skill_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpdateNurseSkillsCommand { Skills = ["Triage"] }, CancellationToken.None));
    }

    private UpdateNurseSkillsCommandHandler CreateHandler()
    {
        return new UpdateNurseSkillsCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseSkill> skills,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseSkill>>? skillMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseSkills).Returns(skillMock?.Object ?? skills.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Languages).Returns(Array.Empty<Language>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

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
