using NursingPlatform.WebApi.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.UseApplicationPipeline();

    await app.InitializeDatabaseAsync();

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
