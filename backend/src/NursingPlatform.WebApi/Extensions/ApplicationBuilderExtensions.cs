using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Identity.Commands.ForgotPassword;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Application.Identity.Commands.Register;
using NursingPlatform.Application.Identity.Commands.RotateRefreshToken;
using NursingPlatform.Application.Identity.Commands.ResetPassword;
using NursingPlatform.Application.Identity.Commands.SendVerificationEmail;
using NursingPlatform.Application.Identity.Commands.VerifyEmail;
using NursingPlatform.Application.Identity.Queries.GetCurrentUser;
using NursingPlatform.Application.Identity.Queries.GetUser;
using NursingPlatform.Application.Identity.Queries.ListUsers;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.WebApi.Middleware;
using Serilog;

namespace NursingPlatform.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(
        this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter
        });

        return app;
    }

    public static WebApplication MapApiEndpoints(
        this WebApplication app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapPost("/auth/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("Login")
        .AllowAnonymous();

        api.MapPost("/auth/refresh", async (RotateRefreshTokenCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .AllowAnonymous();

        api.MapPost("/auth/register", async (RegisterUserRequest request, ISender sender) =>
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleIds = request.RoleIds
            };

            var userId = await sender.Send(command);

            return Results.Ok(new RegisterUserResponse { UserId = userId });
        })
        .WithName("RegisterUser")
        .RequirePermission(Permissions.Users.Create);

        api.MapPost("/auth/send-verification-email", async (ISender sender) =>
        {
            var result = await sender.Send(new SendVerificationEmailCommand());
            return Results.Ok(result);
        })
        .WithName("SendVerificationEmail")
        .RequireAuthorization();

        api.MapPost("/auth/verify-email", async (VerifyEmailRequest request, ISender sender) =>
        {
            var command = new VerifyEmailCommand { Token = request.Token };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("VerifyEmail")
        .AllowAnonymous();

        api.MapPost("/auth/forgot-password", async (ForgotPasswordRequest request, ISender sender) =>
        {
            var command = new ForgotPasswordCommand { Email = request.Email };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("ForgotPassword")
        .AllowAnonymous();

        api.MapPost("/auth/reset-password", async (ResetPasswordRequest request, ISender sender) =>
        {
            var command = new ResetPasswordCommand
            {
                Email = request.Email,
                Token = request.Token,
                NewPassword = request.NewPassword
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("ResetPassword")
        .AllowAnonymous();

        api.MapGet("/me", async (ISender sender) =>
        {
            var user = await sender.Send(new GetCurrentUserQuery());
            return Results.Ok(user);
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization();

        api.MapGet("/users", async (
            int? page,
            int? pageSize,
            string? search,
            bool? isActive,
            string? role,
            string? sort,
            ISender sender) =>
        {
            var query = new ListUsersQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Search = search,
                IsActive = isActive,
                Role = role,
                Sort = sort
            };
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("ListUsers")
        .RequirePermission(Permissions.Users.View);

        api.MapGet("/users/{id:guid}", async (Guid id, ISender sender) =>
        {
            var user = await sender.Send(new GetUserQuery { UserId = id });
            return Results.Ok(user);
        })
        .WithName("GetUser")
        .RequirePermission(Permissions.Users.View);

        return app;
    }

    public static async Task InitializeDatabaseAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }

    private static Task HealthCheckResponseWriter(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
