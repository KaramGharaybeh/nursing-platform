using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Infrastructure.Authentication;

namespace NursingPlatform.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static IConfiguration CreateMinimalConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterCurrentUserService()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CreateMinimalConfiguration());

        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<ICurrentUserService>();
        Assert.IsType<CurrentUserService>(service);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterHttpContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CreateMinimalConfiguration());

        var provider = services.BuildServiceProvider();

        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        Assert.NotNull(accessor);
    }

    [Fact]
    public void CurrentUserService_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CreateMinimalConfiguration());

        var registration = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ICurrentUserService) &&
            s.ImplementationType == typeof(CurrentUserService));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
