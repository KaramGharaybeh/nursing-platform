using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Caching;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Infrastructure.Authentication;
using NursingPlatform.Infrastructure.Caching;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Notifications;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.Infrastructure.Storage;

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

        services.AddOptions<FileStorageSettings>()
            .Bind(configuration.GetSection(FileStorageSettings.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<AdminSettings>()
            .Bind(configuration.GetSection(AdminSettings.SectionName))
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

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<BootstrapAdminService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
