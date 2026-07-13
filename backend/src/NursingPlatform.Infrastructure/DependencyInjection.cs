using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Caching;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Infrastructure.Authentication;
using NursingPlatform.Infrastructure.Caching;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.Infrastructure.Notifications;
using NursingPlatform.Infrastructure.Payments.Sandbox;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.Infrastructure.Storage;

namespace NursingPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddInfrastructure(configuration, environment: null);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment)
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

        services.AddOptions<PaymentCheckoutSettings>()
            .Bind(configuration.GetSection(PaymentCheckoutSettings.SectionName));

        services.AddOptions<SandboxPaymentSettings>()
            .Bind(configuration.GetSection(SandboxPaymentSettings.SectionName));

        RegisterPaymentCheckoutProvider(services, configuration, environment);

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

    private static void RegisterPaymentCheckoutProvider(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment)
    {
        var providerName = configuration[$"{PaymentCheckoutSettings.SectionName}:Provider"];
        if (!string.Equals(providerName, SandboxPaymentCheckoutProvider.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (environment is not null && environment.IsProduction())
        {
            throw new InvalidOperationException("Sandbox payment checkout provider cannot be enabled in Production.");
        }

        if (environment is null ||
            environment.IsDevelopment() ||
            string.Equals(environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IPaymentCheckoutProvider, SandboxPaymentCheckoutProvider>();
        }
    }
}
