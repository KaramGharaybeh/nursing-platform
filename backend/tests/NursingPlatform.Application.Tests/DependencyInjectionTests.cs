using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterPermissionService()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        services.AddApplication();

        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IPermissionService>();
        Assert.IsType<PermissionService>(service);
    }

    [Fact]
    public void AddApplication_ShouldRegisterPermissionAuthorizationHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        services.AddScoped(_ => Mock.Of<ICurrentUserService>());
        services.AddApplication();

        var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IAuthorizationHandler>();
        Assert.IsType<PermissionAuthorizationHandler>(handler);
    }

    [Fact]
    public void PermissionAuthorizationHandler_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var registration = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IAuthorizationHandler) &&
            s.ImplementationType == typeof(PermissionAuthorizationHandler));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    [Fact]
    public void PermissionService_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var registration = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IPermissionService) &&
            s.ImplementationType == typeof(PermissionService));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    [Fact]
    public void AddApplication_ShouldRegisterNurseRoleGuard()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        services.AddScoped(_ => Mock.Of<ICurrentUserService>());
        services.AddApplication();

        var provider = services.BuildServiceProvider();

        var guard = provider.GetRequiredService<NurseRoleGuard>();
        Assert.IsType<NurseRoleGuard>(guard);
    }

    [Fact]
    public void NurseRoleGuard_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var registration = services.FirstOrDefault(s =>
            s.ServiceType == typeof(NurseRoleGuard) &&
            s.ImplementationType == typeof(NurseRoleGuard));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    [Fact]
    public void AddApplication_ShouldRegisterEmployerRoleGuard()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        services.AddScoped(_ => Mock.Of<ICurrentUserService>());
        services.AddApplication();

        var provider = services.BuildServiceProvider();

        var guard = provider.GetRequiredService<EmployerRoleGuard>();
        Assert.IsType<EmployerRoleGuard>(guard);
    }

    [Fact]
    public void EmployerRoleGuard_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var registration = services.FirstOrDefault(s =>
            s.ServiceType == typeof(EmployerRoleGuard) &&
            s.ImplementationType == typeof(EmployerRoleGuard));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
