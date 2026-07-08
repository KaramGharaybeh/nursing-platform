using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

public class WebApiTestFactory : WebApplicationFactory<global::Program>
{
    private static readonly Dictionary<string, string?> _testConfig = new()
    {
        ["Jwt__Secret"] = "test-secret-key-that-is-at-least-32-characters-long",
        ["Jwt__Issuer"] = "TestIssuer",
        ["Jwt__Audience"] = "TestAudience",
        ["Jwt__KeyId"] = "nursing-platform-key",
        ["Jwt__ExpirationInMinutes"] = "60",
        ["Jwt__RefreshTokenExpirationInDays"] = "7",
        ["Database__ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test",
        ["Redis__ConnectionString"] = "",
        ["Admin__Email"] = "admin@nursingplatform.test",
        ["Admin__Password"] = "AdminPass123!",
        ["Admin__FirstName"] = "Test",
        ["Admin__LastName"] = "Test"
    };

    static WebApiTestFactory()
    {
        foreach (var kvp in _testConfig)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }

    public Mock<ISender> SenderMock { get; } = new();

    public Mock<IPermissionService> PermissionServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped(_ => SenderMock.Object);

            services.RemoveAll<IPermissionService>();
            services.AddScoped(_ => PermissionServiceMock.Object);

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
