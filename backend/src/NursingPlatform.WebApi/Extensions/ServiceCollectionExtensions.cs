using NursingPlatform.Application;
using NursingPlatform.Infrastructure;

namespace NursingPlatform.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation(configuration);

        return services;
    }

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddDbContextCheck<Infrastructure.Persistence.ApplicationDbContext>(
            name: "postgresql",
            tags: ["ready"]);

        var redisConnectionString = configuration["Redis:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: ["ready"]);
        }

        services.AddOpenApi();

        return services;
    }
}
