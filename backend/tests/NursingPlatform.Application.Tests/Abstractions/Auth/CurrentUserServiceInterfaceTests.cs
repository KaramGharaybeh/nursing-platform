using Moq;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Application.Tests.Abstractions.Auth;

public class CurrentUserServiceInterfaceTests
{
    [Fact]
    public void Interface_ShouldBeMockable()
    {
        var mock = new Mock<ICurrentUserService>();
        mock.SetupGet(m => m.UserId).Returns((Guid?)null);
        mock.SetupGet(m => m.Email).Returns((string?)null);
        mock.SetupGet(m => m.Roles).Returns(Array.Empty<string>());
        mock.SetupGet(m => m.IsAuthenticated).Returns(false);

        var service = mock.Object;
        Assert.Null(service.UserId);
        Assert.Null(service.Email);
        Assert.Empty(service.Roles);
        Assert.False(service.IsAuthenticated);
    }
}
