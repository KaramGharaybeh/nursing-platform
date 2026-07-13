using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Infrastructure.Authentication;
using NursingPlatform.Infrastructure.Payments.Sandbox;

namespace NursingPlatform.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static IConfiguration CreateMinimalConfiguration(Dictionary<string, string?>? overrides = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test"
        };

        if (overrides is not null)
        {
            foreach (var overrideValue in overrides)
            {
                values[overrideValue.Key] = overrideValue.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
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

    [Theory]
    [InlineData("Development")]
    [InlineData("Test")]
    public void AddInfrastructure_WhenSandboxSelectedInDevelopmentOrTest_RegistersSandboxProvider(string environmentName)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CreateSandboxConfiguration(), new StubHostEnvironment(environmentName));

        var registration = services.SingleOrDefault(s =>
            s.ServiceType == typeof(IPaymentCheckoutProvider) &&
            s.ImplementationType == typeof(SandboxPaymentCheckoutProvider));

        Assert.NotNull(registration);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_WhenSandboxSelectedInProduction_ThrowsSafeConfigurationError()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(CreateSandboxConfiguration(), new StubHostEnvironment(Environments.Production)));

        Assert.Contains("Sandbox payment checkout provider cannot be enabled in Production", exception.Message, StringComparison.Ordinal);
    }

    private static IConfiguration CreateSandboxConfiguration()
    {
        return CreateMinimalConfiguration(new Dictionary<string, string?>
        {
            ["Payment:Checkout:Provider"] = "Sandbox",
            ["Payment:Sandbox:PublicBaseUrl"] = "https://sandbox-payments.local",
            ["Payment:Sandbox:SupportedCurrency"] = "USD"
        });
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "NursingPlatform.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
