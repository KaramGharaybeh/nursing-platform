using NursingPlatform.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseApplicationPipeline();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Application = "Nursing Platform API",
        Version = "v1",
        Status = "Running"
    });
});

app.Run();