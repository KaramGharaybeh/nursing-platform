using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UpdateNurseLanguagesCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task UpdateLanguages_ValidRequest_ReplacesExistingLanguages()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var oldLanguage = new NurseLanguage { Id = Guid.NewGuid(), NurseProfileId = profile.Id, LanguageId = Guid.NewGuid(), Proficiency = "Beginner" };
        var english = CreateLanguage(Guid.NewGuid(), "English", "en", true);
        var arabic = CreateLanguage(Guid.NewGuid(), "Arabic", "ar", true);
        var nurseLanguages = new List<NurseLanguage> { oldLanguage }.AsQueryable().BuildMockDbSet();
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [oldLanguage], [english, arabic], nurseLanguages);
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateNurseLanguagesCommand
        {
            Languages =
            [
                new UpdateNurseLanguageRequest { LanguageId = english.Id, Proficiency = "Fluent" },
                new UpdateNurseLanguageRequest { LanguageId = arabic.Id, Proficiency = "Native" }
            ]
        }, CancellationToken.None);

        nurseLanguages.Verify(l => l.Remove(oldLanguage), Times.Once);
        nurseLanguages.Verify(l => l.Add(It.Is<NurseLanguage>(x =>
            x.NurseProfileId == profile.Id && x.LanguageId == english.Id && x.Proficiency == "Fluent")), Times.Once);
        nurseLanguages.Verify(l => l.Add(It.Is<NurseLanguage>(x =>
            x.NurseProfileId == profile.Id && x.LanguageId == arabic.Id && x.Proficiency == "Native")), Times.Once);
        Assert.Contains(result, item => item.LanguageId == english.Id && item.Name == "English" && item.Code == "en" && item.Proficiency == "Fluent");
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLanguages_InactiveLanguage_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var inactiveLanguage = CreateLanguage(Guid.NewGuid(), "Inactive", "xx", false);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], [inactiveLanguage]);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateNurseLanguagesCommand
            {
                Languages = [new UpdateNurseLanguageRequest { LanguageId = inactiveLanguage.Id, Proficiency = "Beginner" }]
            }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateLanguages_MissingLanguage_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        ConfigureContext(userId, CreateUserWithRole(userId, "Nurse"), [profile], [], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateNurseLanguagesCommand
            {
                Languages = [new UpdateNurseLanguageRequest { LanguageId = Guid.NewGuid(), Proficiency = "Beginner" }]
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Language_NonNurse_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        ConfigureContext(userId, CreateUserWithRole(userId, "Employer"), [], [], []);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpdateNurseLanguagesCommand(), CancellationToken.None));
    }

    private UpdateNurseLanguagesCommandHandler CreateHandler()
    {
        return new UpdateNurseLanguagesCommandHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void ConfigureContext(
        Guid? currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseLanguage> nurseLanguages,
        IReadOnlyCollection<Language> languages,
        Mock<Microsoft.EntityFrameworkCore.DbSet<NurseLanguage>>? nurseLanguageMock = null)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseLanguages).Returns(nurseLanguageMock?.Object ?? nurseLanguages.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Languages).Returns(languages.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static NurseProfile CreateProfile(Guid userId) => new() { Id = Guid.NewGuid(), UserId = userId };

    private static Language CreateLanguage(Guid id, string name, string code, bool isActive)
    {
        return new Language { Id = id, Name = name, Code = code, IsActive = isActive };
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
