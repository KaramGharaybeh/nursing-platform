using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.Register;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class RegisterUserCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsException()
    {
        var existingUser = new User { Email = "existing@test.com" };
        var users = new List<User> { existingUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "existing@test.com" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_InvalidRoleId_ThrowsException()
    {
        _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Roles).Returns(new List<Role>().AsQueryable().BuildMockDbSet().Object);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hash");
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "new@test.com", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { Guid.NewGuid() } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserAndReturnsId()
    {
        var roleId = Guid.NewGuid();
        var roles = new List<Role> { new() { Id = roleId, Name = "Nurse" } }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hash");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "new@test.com", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { roleId } };
        var result = await handler.Handle(command, default);
        Assert.NotEqual(Guid.Empty, result);
        _contextMock.Verify(c => c.Users.Add(It.Is<User>(u =>
            u.PasswordHash == "hash" &&
            u.Email == command.Email &&
            u.FirstName == command.FirstName &&
            u.LastName == command.LastName &&
            u.IsActive &&
            !u.EmailVerified &&
            u.UserRoles.Count == 1 &&
            u.UserRoles.Any(ur => ur.RoleId == roleId))));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_EmptyEmail_ReturnsError()
    {
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { Guid.NewGuid() } };
        Assert.False(v.Validate(c).IsValid);
    }

    [Fact]
    public void Validator_DuplicateRoleIds_ReturnsError()
    {
        var id = Guid.NewGuid();
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "t@t.com", Password = "Password1", FirstName = "J", LastName = "D", RoleIds = new List<Guid> { id, id } };
        Assert.False(v.Validate(c).IsValid);
    }

    [Fact]
    public void Validator_PasswordTooShort_ReturnsError()
    {
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "t@t.com", Password = "short", FirstName = "J", LastName = "D", RoleIds = new List<Guid> { Guid.NewGuid() } };
        Assert.False(v.Validate(c).IsValid);
    }
}
