namespace NursingPlatform.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        return services;
    }
}
