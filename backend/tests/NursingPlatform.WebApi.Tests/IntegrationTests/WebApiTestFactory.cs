using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

public class WebApiTestFactory : WebApplicationFactory<global::Program>
{
    private static readonly Dictionary<string, string?> _testConfig = new()
    {
        ["Jwt:Secret"] = "test-secret-key-that-is-at-least-32-characters-long",
        ["Jwt:Issuer"] = "TestIssuer",
        ["Jwt:Audience"] = "TestAudience",
        ["Jwt:ExpirationInMinutes"] = "60",
        ["Jwt:RefreshTokenExpirationInDays"] = "7",
        ["Database:ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test",
        ["Redis:ConnectionString"] = "",
        ["Admin:Email"] = "admin@nursingplatform.test",
        ["Admin:Password"] = "AdminPass123!",
        ["Admin:FirstName"] = "Test",
        ["Admin:LastName"] = "Admin"
    };

    private static readonly bool _envVarsSet = SetEnvironmentVariables();

    private static bool SetEnvironmentVariables()
    {
        foreach (var kvp in _testConfig)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
        return true;
    }

    public Mock<ISender> SenderMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped(_ => SenderMock.Object);

            services.RemoveAll<DatabaseInitializer>();
            services.AddScoped(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DatabaseInitializer>>();
                var mock = new Mock<DatabaseInitializer>(sp, logger);
                mock.Setup(m => m.InitializeAsync()).Returns(Task.CompletedTask);
                return mock.Object;
            });
        });
    }
}

[CollectionDefinition(Name)]
public class WebApiTestCollection : ICollectionFixture<WebApiTestFactory>
{
    public const string Name = "WebApiTestCollection";
}
