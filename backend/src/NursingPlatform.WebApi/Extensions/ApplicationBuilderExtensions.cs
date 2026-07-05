namespace NursingPlatform.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(
        this WebApplication app)
    {
        app.UseHttpsRedirection();

        return app;
    }
}


