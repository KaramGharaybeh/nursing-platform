using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class UpdateCurrentUserProfileCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    [Fact]
    public async Task Handle_IncompleteProfile_UpdatesFirstAndLastNameAndBecomesComplete()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "new@test.com",
            Username = "new-user",
            NormalizedUsername = "NEW-USER",
            FirstName = string.Empty,
            LastName = string.Empty,
            IsActive = true,
            EmailVerified = true
        };

        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCurrentUserProfileCommandHandler(_currentUserMock.Object, _contextMock.Object);

        var result = await handler.Handle(
            new UpdateCurrentUserProfileCommand { FirstName = "New", LastName = "User" },
            default);

        Assert.Equal("New", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.True(result.IsProfileComplete);
        Assert.Equal("new-user", result.Username);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Unauthenticated_ThrowsUnauthorizedAccessException()
    {
        _currentUserMock.SetupGet(u => u.UserId).Returns((Guid?)null);
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var handler = new UpdateCurrentUserProfileCommandHandler(_currentUserMock.Object, _contextMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new UpdateCurrentUserProfileCommand { FirstName = "New", LastName = "User" }, default));
    }

    [Fact]
    public void Validator_EmptyFirstName_ReturnsError()
    {
        var validator = new UpdateCurrentUserProfileCommandValidator();
        var command = new UpdateCurrentUserProfileCommand { FirstName = string.Empty, LastName = "User" };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_WhitespaceFirstName_ReturnsError()
    {
        var validator = new UpdateCurrentUserProfileCommandValidator();
        var command = new UpdateCurrentUserProfileCommand { FirstName = "   ", LastName = "User" };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_EmptyLastName_ReturnsError()
    {
        var validator = new UpdateCurrentUserProfileCommandValidator();
        var command = new UpdateCurrentUserProfileCommand { FirstName = "New", LastName = string.Empty };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_WhitespaceLastName_ReturnsError()
    {
        var validator = new UpdateCurrentUserProfileCommandValidator();
        var command = new UpdateCurrentUserProfileCommand { FirstName = "New", LastName = "   " };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_OverlengthFirstName_ReturnsError()
    {
        var validator = new UpdateCurrentUserProfileCommandValidator();
        var command = new UpdateCurrentUserProfileCommand { FirstName = new string('A', 101), LastName = "User" };

        Assert.False(validator.Validate(command).IsValid);
    }
}
