using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NursingPlatform.Infrastructure.Authentication;

namespace NursingPlatform.Infrastructure.Tests.Authentication;

public class CurrentUserServiceTests
{
    [Fact]
    public void NoHttpContext_ReturnsSafeDefaults()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Null(service.UserId);
        Assert.Null(service.Email);
        Assert.Empty(service.Roles);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_UnauthenticatedUser_ReturnsFalse()
    {
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void UserId_WithNameIdentifierClaim_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void UserId_WithSubClaim_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", userId.ToString())
        }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void UserId_WithInvalidGuidClaim_ReturnsNull()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Null(service.UserId);
    }

    [Fact]
    public void Email_WithEmailClaim_ReturnsValue()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@test.com")
        }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Equal("user@test.com", service.Email);
    }

    [Fact]
    public void Roles_WithRoleClaims_ReturnsAllRoles()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Nurse")
        }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.Equal(2, service.Roles.Count);
        Assert.Contains("Admin", service.Roles);
        Assert.Contains("Nurse", service.Roles);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(h => h.User).Returns(principal);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.SetupGet(h => h.HttpContext).Returns(httpContextMock.Object);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        Assert.True(service.IsAuthenticated);
    }
}
