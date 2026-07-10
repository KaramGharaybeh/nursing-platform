using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Nurses.Queries;

public class ListCurrentNurseLanguagesQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListLanguages_ReturnsLanguageNameCodeAndProficiency()
    {
        var userId = Guid.NewGuid();
        var profile = new NurseProfile { Id = Guid.NewGuid(), UserId = userId };
        var english = new Language { Id = Guid.NewGuid(), Name = "English", Code = "en", IsActive = true };
        var arabic = new Language { Id = Guid.NewGuid(), Name = "Arabic", Code = "ar", IsActive = true };
        var otherProfileLanguage = new NurseLanguage { Id = Guid.NewGuid(), NurseProfileId = Guid.NewGuid(), LanguageId = english.Id, Proficiency = "Beginner" };
        var nurseLanguage = new NurseLanguage { Id = Guid.NewGuid(), NurseProfileId = profile.Id, LanguageId = arabic.Id, Proficiency = "Native" };
        ConfigureContext(userId, CreateNurseUser(userId), [profile], [otherProfileLanguage, nurseLanguage], [english, arabic]);
        var handler = new ListCurrentNurseLanguagesQueryHandler(
            _contextMock.Object,
            new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));

        var result = await handler.Handle(new ListCurrentNurseLanguagesQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.IsType<NurseLanguageDto>(item);
        Assert.Equal(arabic.Id, item.LanguageId);
        Assert.Equal("Arabic", item.Name);
        Assert.Equal("ar", item.Code);
        Assert.Equal("Native", item.Proficiency);
        Assert.Null(typeof(NurseLanguageDto).GetProperty("NurseProfile"));
        Assert.Null(typeof(NurseLanguageDto).GetProperty("Language"));
    }

    private void ConfigureContext(
        Guid currentUserId,
        User user,
        IReadOnlyCollection<NurseProfile> profiles,
        IReadOnlyCollection<NurseLanguage> nurseLanguages,
        IReadOnlyCollection<Language> languages)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(profiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseLanguages).Returns(nurseLanguages.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Languages).Returns(languages.AsQueryable().BuildMockDbSet().Object);
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
