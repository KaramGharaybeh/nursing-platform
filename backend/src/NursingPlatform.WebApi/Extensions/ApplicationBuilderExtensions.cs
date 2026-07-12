using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerOrganization;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;
using NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Commands.SubmitExamSession;
using NursingPlatform.Application.Exams.Queries.GetExam;
using NursingPlatform.Application.Exams.Queries.GetExamSession;
using NursingPlatform.Application.Exams.Queries.GetExamSessionResult;
using NursingPlatform.Application.Exams.Queries.GetExamSessionReview;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;
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
using NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;
using NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseCv;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseEducation;
using NursingPlatform.Application.Nurses.Commands.DeleteNurseExperience;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseCertificate;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseEducation;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseExperience;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;
using NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;
using NursingPlatform.Application.Nurses.Commands.UploadNurseCv;
using NursingPlatform.Application.Nurses.Commands.UpsertNurseProfile;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;
using NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;
using NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;
using NursingPlatform.Application.Recruitment.Queries.ListCandidates;
using NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;
using NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Recruitment;
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

        api.MapGet("/recruitment/candidates", async (
            int? page,
            int? pageSize,
            Guid? licenseCountryId,
            Guid? currentCountryId,
            int? minimumYearsOfExperience,
            string[]? skills,
            Guid? languageId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListCandidatesQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                LicenseCountryId = licenseCountryId,
                CurrentCountryId = currentCountryId,
                MinimumYearsOfExperience = minimumYearsOfExperience,
                Skills = skills ?? [],
                LanguageId = languageId
            });
            return Results.Ok(result);
        })
        .WithName("ListRecruitmentCandidates")
        .RequireAuthorization();

        api.MapGet("/exams", async (
            int? page,
            int? pageSize,
            Guid? countryId,
            Guid? categoryId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListExamsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                CountryId = countryId,
                CategoryId = categoryId
            });
            return Results.Ok(result);
        })
        .WithName("ListExams")
        .RequireAuthorization();

        api.MapGet("/exams/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamQuery { ExamId = id });
            return Results.Ok(result);
        })
        .WithName("GetExam")
        .RequireAuthorization();

        api.MapPost("/exams/{id:guid}/sessions", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new StartExamSessionCommand { ExamId = id });
            return Results.Ok(result);
        })
        .WithName("StartExamSession")
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSession")
        .RequireAuthorization();

        api.MapPut("/exam-sessions/{id:guid}/answers", async (
            Guid id,
            SaveExamSessionAnswersRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new SaveExamSessionAnswersCommand
            {
                ExamSessionId = id,
                Request = request
            });
            return Results.Ok(result);
        })
        .WithName("SaveExamSessionAnswers")
        .RequireAuthorization();

        api.MapPost("/exam-sessions/{id:guid}/submit", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new SubmitExamSessionCommand { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("SubmitExamSession")
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}/result", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionResultQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSessionResult")
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}/review", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionReviewQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSessionReview")
        .RequireAuthorization();

        api.MapPost("/recruitment/contact-requests", async (CreateContactRequestRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateContactRequestCommand
            {
                NurseProfileId = request.NurseProfileId
            });
            return Results.Created($"/api/v1/recruitment/contact-requests/{result.Id}", result);
        })
        .WithName("CreateRecruitmentContactRequest")
        .RequireAuthorization();

        api.MapGet("/recruitment/contact-requests", async (
            int? page,
            int? pageSize,
            ContactRequestStatus? status,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyContactRequestsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Status = status
            });
            return Results.Ok(result);
        })
        .WithName("ListMyRecruitmentContactRequests")
        .RequireAuthorization();

        api.MapGet("/recruitment/contact-requests/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetMyContactRequestQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("GetMyRecruitmentContactRequest")
        .RequireAuthorization();

        api.MapPost("/recruitment/contact-requests/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("CancelRecruitmentContactRequest")
        .RequireAuthorization();

        var employerProfile = api.MapGroup("/me/employer-profile")
            .RequireAuthorization();

        employerProfile.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMyEmployerProfileQuery());
            return Results.Ok(result);
        })
        .WithName("GetMyEmployerProfile");

        employerProfile.MapPut("/", async (UpsertMyEmployerProfileRequest request, ISender sender) =>
        {
            var command = new UpsertMyEmployerProfileCommand
            {
                JobTitle = request.JobTitle,
                Department = request.Department
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpsertMyEmployerProfile");

        employerProfile.MapGet("/organization", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMyEmployerOrganizationQuery());
            return Results.Ok(result);
        })
        .WithName("GetMyEmployerOrganization");

        employerProfile.MapPut("/organization", async (UpsertMyEmployerOrganizationRequest request, ISender sender) =>
        {
            var command = new UpsertMyEmployerOrganizationCommand
            {
                Name = request.Name,
                Type = request.Type,
                WebsiteUrl = request.WebsiteUrl,
                CountryId = request.CountryId,
                City = request.City,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                PostalCode = request.PostalCode,
                Description = request.Description
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpsertMyEmployerOrganization");

        var nurseProfile = api.MapGroup("/me/nurse-profile")
            .RequireAuthorization();

        nurseProfile.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCurrentNurseProfileQuery());
            return Results.Ok(result);
        })
        .WithName("GetCurrentNurseProfile");

        nurseProfile.MapPut("/", async (UpsertNurseProfileCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpsertCurrentNurseProfile");

        nurseProfile.MapGet("/experiences", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseExperiencesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseExperiences");

        nurseProfile.MapPost("/experiences", async (CreateNurseExperienceCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseExperience");

        nurseProfile.MapPut("/experiences/{id:guid}", async (Guid id, UpdateNurseExperienceCommand request, ISender sender) =>
        {
            var command = new UpdateNurseExperienceCommand
            {
                Id = id,
                FacilityName = request.FacilityName,
                JobTitle = request.JobTitle,
                CountryId = request.CountryId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsCurrent = request.IsCurrent,
                Description = request.Description
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseExperience");

        nurseProfile.MapDelete("/experiences/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseExperienceCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseExperience");

        nurseProfile.MapGet("/education", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseEducationQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseEducation");

        nurseProfile.MapPost("/education", async (CreateNurseEducationCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseEducation");

        nurseProfile.MapPut("/education/{id:guid}", async (Guid id, UpdateNurseEducationCommand request, ISender sender) =>
        {
            var command = new UpdateNurseEducationCommand
            {
                Id = id,
                InstitutionName = request.InstitutionName,
                Degree = request.Degree,
                FieldOfStudy = request.FieldOfStudy,
                CountryId = request.CountryId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Description = request.Description
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseEducation");

        nurseProfile.MapDelete("/education/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseEducationCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseEducation");

        nurseProfile.MapGet("/certificates", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseCertificatesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseCertificates");

        nurseProfile.MapPost("/certificates", async (CreateNurseCertificateCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseCertificate");

        nurseProfile.MapPut("/certificates/{id:guid}", async (Guid id, UpdateNurseCertificateCommand request, ISender sender) =>
        {
            var command = new UpdateNurseCertificateCommand
            {
                Id = id,
                Name = request.Name,
                IssuingOrganization = request.IssuingOrganization,
                IssueDate = request.IssueDate,
                ExpirationDate = request.ExpirationDate,
                CredentialId = request.CredentialId,
                CredentialUrl = request.CredentialUrl
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseCertificate");

        nurseProfile.MapDelete("/certificates/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseCertificateCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseCertificate");

        nurseProfile.MapGet("/languages", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseLanguagesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseLanguages");

        nurseProfile.MapPut("/languages", async (UpdateNurseLanguagesCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseLanguages");

        nurseProfile.MapGet("/skills", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseSkillsQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseSkills");

        nurseProfile.MapPut("/skills", async (UpdateNurseSkillsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseSkills");

        nurseProfile.MapGet("/contact-requests", async (
            int? page,
            int? pageSize,
            ContactRequestStatus? status,
            ISender sender) =>
        {
            var result = await sender.Send(new ListReceivedContactRequestsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Status = status
            });
            return Results.Ok(result);
        })
        .WithName("ListReceivedContactRequests");

        nurseProfile.MapPost("/contact-requests/{id:guid}/approve", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ApproveReceivedContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("ApproveReceivedContactRequest");

        nurseProfile.MapPost("/contact-requests/{id:guid}/reject", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RejectReceivedContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("RejectReceivedContactRequest");

        nurseProfile.MapGet("/exam-attempts", async (
            int? page,
            int? pageSize,
            ExamSessionStatus? status,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyExamAttemptsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Status = status
            });
            return Results.Ok(result);
        })
        .WithName("ListMyExamAttempts");

        nurseProfile.MapGet("/cv", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCurrentNurseCvQuery());
            return Results.Ok(result);
        })
        .WithName("GetCurrentNurseCv");

        nurseProfile.MapPost("/cv", async (HttpRequest request, ISender sender) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest();
            }

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");

            if (file is null)
            {
                return Results.BadRequest();
            }

            var command = new UploadNurseCvCommand
            {
                File = file.OpenReadStream(),
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length
            };
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UploadNurseCv");

        nurseProfile.MapDelete("/cv", async (ISender sender) =>
        {
            await sender.Send(new DeleteNurseCvCommand());
            return Results.NoContent();
        })
        .WithName("DeleteNurseCv");

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
