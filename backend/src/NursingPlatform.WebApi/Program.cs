using NursingPlatform.WebApi;
using NursingPlatform.WebApi.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var openApiCaptureMode = OpenApiCaptureMode.FromRawArguments(args);
    var builder = WebApplication.CreateBuilder(openApiCaptureMode.RemainingArguments);

    if (openApiCaptureMode.IsEnabled && !builder.Environment.IsDevelopment())
    {
        Environment.ExitCode = 1;
        openApiCaptureMode.EnsureDevelopmentOnly(isDevelopment: false);
    }

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.UseApplicationPipeline();

    if (!openApiCaptureMode.IsEnabled)
    {
        await app.InitializeDatabaseAsync();
    }

    app.MapGet("/", () =>
    {
        return Results.Ok(new
        {
            Application = "Nursing Platform API",
            Version = "v1",
            Status = "Running"
        });
    });

    app.MapApiEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
