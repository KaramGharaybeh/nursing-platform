using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Infrastructure.Authentication;

namespace NursingPlatform.Infrastructure.Tests.Authentication;

public class PasswordHashingServiceTests
{
    private readonly IPasswordHashingService _sut = new PasswordHashingService();

    [Fact]
    public void Hash_ReturnsNonEmptyString_NotEqualToPlaintext()
    {
        const string password = "CorrectPass1";

        var hash = _sut.Hash(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Verify_ValidPassword_ReturnsTrue()
    {
        const string password = "CorrectPass1";
        var hash = _sut.Hash(password);

        var result = _sut.Verify(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_InvalidPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("CorrectPass1");

        var result = _sut.Verify("WrongPassword", hash);

        Assert.False(result);
    }
}
