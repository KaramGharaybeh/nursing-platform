using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NursingPlatform.Application.Abstractions.Caching;
using NursingPlatform.Infrastructure.Caching;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(DatabaseSettings.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<RedisSettings>()
            .Bind(configuration.GetSection(RedisSettings.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations();

        var redisConnectionString = configuration
            .GetSection(RedisSettings.SectionName)
            .Get<RedisSettings>()?
            .ConnectionString;

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
                options.Configuration = redisConnectionString);

            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddScoped<ICacheService, NoOpCacheService>();
        }

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var databaseSettings = sp
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseSettings>>()
                .Value;

            options.UseNpgsql(
                databaseSettings.ConnectionString,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(DependencyInjection).Assembly.FullName));
        });

        return services;
    }
}
