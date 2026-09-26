using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerOrganization;
using NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerOrganization;
using NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;
using NursingPlatform.Application.Exams.Commands.SaveExamSessionAnswers;
using NursingPlatform.Application.Exams.Commands.StartExamSession;
using NursingPlatform.Application.Exams.Commands.SubmitExamSession;
using NursingPlatform.Application.Exams.Admin.AnswerOptions;
using NursingPlatform.Application.Exams.Admin.Categories;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Application.Exams.Admin.Exams;
using NursingPlatform.Application.Exams.Admin.Questions;
using NursingPlatform.Application.Exams.Admin.Versions;
using NursingPlatform.Application.Exams.Analytics.Common;
using NursingPlatform.Application.Exams.Analytics.DTOs;
using NursingPlatform.Application.Exams.Analytics.Queries.GetMyExamAnalyticsSummary;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByCategory;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsByExam;
using NursingPlatform.Application.Exams.Analytics.Queries.ListMyExamAnalyticsTrends;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Exams.Queries.GetExam;
using NursingPlatform.Application.Exams.Queries.GetExamSession;
using NursingPlatform.Application.Exams.Queries.GetExamSessionResult;
using NursingPlatform.Application.Exams.Queries.GetExamSessionReview;
using NursingPlatform.Application.Exams.Queries.ListExams;
using NursingPlatform.Application.Exams.Queries.ListMyExamAttempts;
using NursingPlatform.Application.Identity.Commands.ForgotPassword;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Application.Identity.Commands.PublicRegister;
using NursingPlatform.Application.Identity.Commands.Register;
using NursingPlatform.Application.Identity.Commands.RotateRefreshToken;
using NursingPlatform.Application.Identity.Commands.ResetPassword;
using NursingPlatform.Application.Identity.Commands.SendVerificationEmail;
using NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;
using NursingPlatform.Application.Identity.Commands.UpdateUserRoles;
using NursingPlatform.Application.Identity.Commands.VerifyEmail;
using NursingPlatform.Application.Identity.Common;
using NursingPlatform.Application.Identity.DTOs;
using NursingPlatform.Application.Identity.Queries.GetCurrentUser;
using NursingPlatform.Application.Identity.Queries.GetUser;
using NursingPlatform.Application.Identity.Queries.ListUsers;
using NursingPlatform.Application.Common.Models;
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
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;
using NursingPlatform.Application.Nurses.Queries.GetCurrentNurseProfile;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;
using NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;
using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CancelMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.CompleteSandboxPaymentCheckout;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Application.Payments.Queries.GetMyPaymentOrder;
using NursingPlatform.Application.Payments.Queries.GetPaymentProduct;
using NursingPlatform.Application.Payments.Queries.ListMyPaymentOrders;
using NursingPlatform.Application.Payments.Queries.ListPaymentProducts;
using NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;
using NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;
using NursingPlatform.Application.ReferenceData.Countries.DTOs;
using NursingPlatform.Application.ReferenceData.Countries.Queries.ListCountries;
using NursingPlatform.Application.ReferenceData.Languages.DTOs;
using NursingPlatform.Application.ReferenceData.Languages.Queries.ListLanguages;
using NursingPlatform.Application.Recruitment.Queries.ListCandidates;
using NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;
using NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Infrastructure.Persistence;
using NursingPlatform.WebApi.Contracts;
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

        if (app.Environment.IsDevelopment())
        {
            app.UseCors(ServiceCollectionExtensions.LocalDevelopmentCorsPolicy);
        }

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

        api.MapPreparationPackageEndpoints();

        api.MapPost("/auth/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("Login")
        .Produces<AuthResult>(StatusCodes.Status200OK)
        .Produces<CodedProblemDetailsContract>(StatusCodes.Status403Forbidden, "application/problem+json")
        .AllowAnonymous();

        api.MapPost("/auth/refresh", async (RotateRefreshTokenCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .Produces<AuthResult>(StatusCodes.Status200OK)
        .AllowAnonymous();

        api.MapPost("/auth/register", async (RegisterUserRequest request, ISender sender) =>
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Username = request.Username,
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

        api.MapPost("/auth/sign-up", async (PublicRegisterRequest request, ISender sender) =>
        {
            await sender.Send(new PublicRegisterCommand
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                RoleName = PublicRegistrationRoleNames.Nurse
            });

            return Results.Accepted();
        })
        .WithName("PublicSignUp")
        .Produces(StatusCodes.Status202Accepted)
        .AllowAnonymous();

        api.MapPost("/auth/register/nurse", () => Results.StatusCode(StatusCodes.Status410Gone))
        .WithName("PublicRegisterNurseRetired")
        .Produces(StatusCodes.Status410Gone)
        .AllowAnonymous();

        api.MapPost("/auth/register/employer", () => Results.StatusCode(StatusCodes.Status410Gone))
        .WithName("PublicRegisterEmployerRetired")
        .Produces(StatusCodes.Status410Gone)
        .AllowAnonymous();

        api.MapPut("/me/profile", async (UpdateCurrentUserProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateCurrentUserProfileCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName
            });

            return Results.Ok(result);
        })
        .WithName("UpdateCurrentUserProfile")
        .Produces<UpdateCurrentUserProfileResponse>(StatusCodes.Status200OK)
        .RequireAuthorization();

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
        .Produces<UserDetailDto>(StatusCodes.Status200OK)
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
        .Produces<PaginatedResult<UserListItemDto>>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Users.View);

        api.MapGet("/users/{id:guid}", async (Guid id, ISender sender) =>
        {
            var user = await sender.Send(new GetUserQuery { UserId = id });
            return Results.Ok(user);
        })
        .WithName("GetUser")
        .Produces<UserDetailDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Users.View);

        api.MapPut("/admin/users/{userId:guid}/role", async (
            Guid userId,
            UpdateUserRolesRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new UpdateUserRolesCommand
            {
                UserId = userId,
                RoleName = request.RoleName
            });

            return Results.Ok(result);
        })
        .WithName("UpdateAdminUserRole")
        .Produces<UpdateUserRolesResponse>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Users.Edit);

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
        .Produces<PaginatedResult<CandidateListItemDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/countries", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCountriesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCountries")
        .Produces<IReadOnlyList<CountryListItemDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/languages", async (ISender sender) =>
        {
            var result = await sender.Send(new ListLanguagesQuery());
            return Results.Ok(result);
        })
        .WithName("ListLanguages")
        .Produces<IReadOnlyList<LanguageListItemDto>>(StatusCodes.Status200OK)
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
        .Produces<PaginatedResult<ExamCatalogItemDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/exams/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamQuery { ExamId = id });
            return Results.Ok(result);
        })
        .WithName("GetExam")
        .Produces<ExamDetailDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/payment/products", async (
            int? page,
            int? pageSize,
            Guid? examId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListPaymentProductsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                ExamId = examId
            });
            return Results.Ok(result);
        })
        .WithName("ListPaymentProducts")
        .Produces<PaginatedResult<PaymentProductDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/payment/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetPaymentProductQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("GetPaymentProduct")
        .Produces<PaymentProductDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapPost("/exams/{id:guid}/sessions", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new StartExamSessionCommand { ExamId = id });
            return Results.Ok(result);
        })
        .WithName("StartExamSession")
        .Produces<ExamSessionDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSession")
        .Produces<ExamSessionDto>(StatusCodes.Status200OK)
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
        .Produces<ExamSessionDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapPost("/exam-sessions/{id:guid}/submit", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new SubmitExamSessionCommand { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("SubmitExamSession")
        .Produces<ExamSessionResultDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}/result", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionResultQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSessionResult")
        .Produces<ExamSessionResultDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/exam-sessions/{id:guid}/review", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExamSessionReviewQuery { ExamSessionId = id });
            return Results.Ok(result);
        })
        .WithName("GetExamSessionReview")
        .Produces<ExamSessionReviewDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        var admin = api.MapGroup("/admin");

        admin.MapGet("/exam-categories", async (
            int? page,
            int? pageSize,
            Guid? countryId,
            bool? isActive,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminExamCategoriesQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                CountryId = countryId,
                IsActive = isActive
            });
            return Results.Ok(result);
        })
        .WithName("AdminListExamCategories")
        .Produces<PaginatedResult<AdminExamCategoryDto>>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.View);

        admin.MapGet("/exam-categories/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminExamCategoryQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminGetExamCategory")
        .Produces<AdminExamCategoryDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.View);

        admin.MapPost("/exam-categories", async (CreateAdminExamCategoryRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminExamCategoryCommand { Request = request });
            return Results.Created($"/api/v1/admin/exam-categories/{result.Id}", result);
        })
        .WithName("AdminCreateExamCategory")
        .Produces<AdminExamCategoryDto>(StatusCodes.Status201Created)
        .RequirePermission(Permissions.Exams.Create);

        admin.MapPut("/exam-categories/{id:guid}", async (Guid id, UpdateAdminExamCategoryRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminExamCategoryCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdateExamCategory")
        .Produces<AdminExamCategoryDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/exam-categories/{id:guid}/archive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveAdminExamCategoryCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminArchiveExamCategory")
        .Produces<AdminExamCategoryDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/exam-categories/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RestoreAdminExamCategoryCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminRestoreExamCategory")
        .Produces<AdminExamCategoryDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapDelete("/exam-categories/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteAdminExamCategoryCommand { Id = id });
            return Results.NoContent();
        })
        .WithName("AdminDeleteExamCategory")
        .Produces(StatusCodes.Status204NoContent)
        .RequirePermission(Permissions.Exams.Delete);

        admin.MapGet("/exams", async (
            int? page,
            int? pageSize,
            Guid? countryId,
            Guid? categoryId,
            ExamStatus? status,
            bool? isFree,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminExamsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                CountryId = countryId,
                CategoryId = categoryId,
                Status = status,
                IsFree = isFree
            });
            return Results.Ok(result);
        })
        .WithName("AdminListExams")
        .Produces<PaginatedResult<AdminExamDto>>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.View);

        admin.MapGet("/exams/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminExamQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminGetExam")
        .Produces<AdminExamDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.View);

        admin.MapPost("/exams", async (CreateAdminExamRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminExamCommand { Request = request });
            return Results.Created($"/api/v1/admin/exams/{result.Id}", result);
        })
        .WithName("AdminCreateExam")
        .Produces<AdminExamDto>(StatusCodes.Status201Created)
        .RequirePermission(Permissions.Exams.Create);

        admin.MapPut("/exams/{id:guid}", async (Guid id, UpdateAdminExamRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminExamCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdateExam")
        .Produces<AdminExamDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/exams/{id:guid}/archive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveAdminExamCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminArchiveExam")
        .Produces<AdminExamDto>(StatusCodes.Status200OK)
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapDelete("/exams/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteAdminExamCommand { Id = id });
            return Results.NoContent();
        })
        .WithName("AdminDeleteExam")
        .Produces(StatusCodes.Status204NoContent)
        .RequirePermission(Permissions.Exams.Delete);

        admin.MapGet("/payment/products", async (
            int? page,
            int? pageSize,
            Guid? examId,
            bool? isActive,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminPaymentProductsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                ExamId = examId,
                IsActive = isActive
            });
            return Results.Ok(result);
        })
        .WithName("AdminListPaymentProducts")
        .RequirePermission(Permissions.Exams.View);

        admin.MapGet("/payment/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminPaymentProductQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminGetPaymentProduct")
        .RequirePermission(Permissions.Exams.View);

        admin.MapPost("/payment/products", async (CreateAdminPaymentProductRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPaymentProductCommand { Request = request });
            return Results.Created($"/api/v1/admin/payment/products/{result.Id}", result);
        })
        .WithName("AdminCreatePaymentProduct")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPut("/payment/products/{id:guid}", async (Guid id, UpdateAdminPaymentProductRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPaymentProductCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePaymentProduct")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/payment/products/{id:guid}/archive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveAdminPaymentProductCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminArchivePaymentProduct")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/payment/products/{id:guid}/restore", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RestoreAdminPaymentProductCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminRestorePaymentProduct")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapGet("/exams/{examId:guid}/versions", async (Guid examId, ISender sender) =>
        {
            var result = await sender.Send(new ListAdminExamVersionsQuery { ExamId = examId });
            return Results.Ok(result);
        })
        .WithName("AdminListExamVersions")
        .RequirePermission(Permissions.Exams.View);

        admin.MapGet("/exams/{examId:guid}/versions/{versionId:guid}", async (Guid examId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminExamVersionQuery { ExamId = examId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminGetExamVersion")
        .RequirePermission(Permissions.Exams.View);

        admin.MapPost("/exams/{examId:guid}/versions", async (Guid examId, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminDraftExamVersionCommand { ExamId = examId });
            return Results.Created($"/api/v1/admin/exams/{examId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreateDraftExamVersion")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/validate", async (Guid examId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new ValidateAdminDraftExamVersionCommand { ExamId = examId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminValidateDraftExamVersion")
        .RequirePermission(Permissions.Questions.View);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/publish", async (Guid examId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminDraftExamVersionCommand { ExamId = examId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishDraftExamVersion")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/retire", async (Guid examId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminExamVersionCommand { ExamId = examId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetireExamVersion")
        .RequirePermission(Permissions.Exams.Edit);

        admin.MapDelete("/exams/{examId:guid}/versions/{versionId:guid}", async (Guid examId, Guid versionId, ISender sender) =>
        {
            await sender.Send(new DeleteAdminDraftExamVersionCommand { ExamId = examId, VersionId = versionId });
            return Results.NoContent();
        })
        .WithName("AdminDeleteDraftExamVersion")
        .RequirePermission(Permissions.Exams.Delete);

        admin.MapGet("/exams/{examId:guid}/versions/{versionId:guid}/questions", async (Guid examId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new ListAdminExamQuestionsQuery { ExamId = examId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminListExamQuestions")
        .RequirePermission(Permissions.Questions.View);

        admin.MapGet("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}", async (Guid examId, Guid versionId, Guid questionId, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminExamQuestionQuery { ExamId = examId, VersionId = versionId, QuestionId = questionId });
            return Results.Ok(result);
        })
        .WithName("AdminGetExamQuestion")
        .RequirePermission(Permissions.Questions.View);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/questions", async (Guid examId, Guid versionId, UpsertAdminExamQuestionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminExamQuestionCommand { ExamId = examId, VersionId = versionId, Request = request });
            return Results.Created($"/api/v1/admin/exams/{examId}/versions/{versionId}/questions/{result.Id}", result);
        })
        .WithName("AdminCreateExamQuestion")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapPut("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}", async (Guid examId, Guid versionId, Guid questionId, UpsertAdminExamQuestionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminExamQuestionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdateExamQuestion")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/deactivate", async (Guid examId, Guid versionId, Guid questionId, ISender sender) =>
        {
            var result = await sender.Send(new DeactivateAdminExamQuestionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId });
            return Results.Ok(result);
        })
        .WithName("AdminDeactivateExamQuestion")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapDelete("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}", async (Guid examId, Guid versionId, Guid questionId, ISender sender) =>
        {
            await sender.Send(new DeleteAdminExamQuestionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId });
            return Results.NoContent();
        })
        .WithName("AdminDeleteExamQuestion")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapGet("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options", async (Guid examId, Guid versionId, Guid questionId, ISender sender) =>
        {
            var result = await sender.Send(new ListAdminExamAnswerOptionsQuery { ExamId = examId, VersionId = versionId, QuestionId = questionId });
            return Results.Ok(result);
        })
        .WithName("AdminListExamAnswerOptions")
        .RequirePermission(Permissions.Questions.View);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options", async (Guid examId, Guid versionId, Guid questionId, UpsertAdminExamAnswerOptionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminExamAnswerOptionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId, Request = request });
            return Results.Created($"/api/v1/admin/exams/{examId}/versions/{versionId}/questions/{questionId}/options/{result.Id}", result);
        })
        .WithName("AdminCreateExamAnswerOption")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapPut("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}", async (Guid examId, Guid versionId, Guid questionId, Guid optionId, UpsertAdminExamAnswerOptionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminExamAnswerOptionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId, OptionId = optionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdateExamAnswerOption")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapPost("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}/deactivate", async (Guid examId, Guid versionId, Guid questionId, Guid optionId, ISender sender) =>
        {
            var result = await sender.Send(new DeactivateAdminExamAnswerOptionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId, OptionId = optionId });
            return Results.Ok(result);
        })
        .WithName("AdminDeactivateExamAnswerOption")
        .RequirePermission(Permissions.Questions.Manage);

        admin.MapDelete("/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}", async (Guid examId, Guid versionId, Guid questionId, Guid optionId, ISender sender) =>
        {
            await sender.Send(new DeleteAdminExamAnswerOptionCommand { ExamId = examId, VersionId = versionId, QuestionId = questionId, OptionId = optionId });
            return Results.NoContent();
        })
        .WithName("AdminDeleteExamAnswerOption")
        .RequirePermission(Permissions.Questions.Manage);

        api.MapPost("/recruitment/contact-requests", async (CreateContactRequestRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateContactRequestCommand
            {
                NurseProfileId = request.NurseProfileId
            });
            return Results.Created($"/api/v1/recruitment/contact-requests/{result.Id}", result);
        })
        .WithName("CreateRecruitmentContactRequest")
        .Produces<ContactRequestDto>(StatusCodes.Status201Created)
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
        .Produces<PaginatedResult<ContactRequestDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapGet("/recruitment/contact-requests/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetMyContactRequestQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("GetMyRecruitmentContactRequest")
        .Produces<ContactRequestDto>(StatusCodes.Status200OK)
        .RequireAuthorization();

        api.MapPost("/recruitment/contact-requests/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("CancelRecruitmentContactRequest")
        .Produces<ContactRequestDto>(StatusCodes.Status200OK)
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
        .WithName("GetCurrentNurseProfile")
        .Produces<NurseProfileDto>(StatusCodes.Status200OK);

        nurseProfile.MapPut("/", async (UpsertNurseProfileCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpsertCurrentNurseProfile")
        .Produces<NurseProfileDto>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/experiences", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseExperiencesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseExperiences")
        .Produces<IReadOnlyList<NurseExperienceDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/experiences", async (CreateNurseExperienceCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseExperience")
        .Produces<NurseExperienceDto>(StatusCodes.Status200OK);

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
        .WithName("UpdateNurseExperience")
        .Produces<NurseExperienceDto>(StatusCodes.Status200OK);

        nurseProfile.MapDelete("/experiences/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseExperienceCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseExperience")
        .Produces(StatusCodes.Status204NoContent);

        nurseProfile.MapGet("/education", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseEducationQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseEducation")
        .Produces<IReadOnlyList<NurseEducationDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/education", async (CreateNurseEducationCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseEducation")
        .Produces<NurseEducationDto>(StatusCodes.Status200OK);

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
        .WithName("UpdateNurseEducation")
        .Produces<NurseEducationDto>(StatusCodes.Status200OK);

        nurseProfile.MapDelete("/education/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseEducationCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseEducation")
        .Produces(StatusCodes.Status204NoContent);

        nurseProfile.MapGet("/certificates", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseCertificatesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseCertificates")
        .Produces<IReadOnlyList<NurseCertificateDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/certificates", async (CreateNurseCertificateCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("CreateNurseCertificate")
        .Produces<NurseCertificateDto>(StatusCodes.Status200OK);

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
        .WithName("UpdateNurseCertificate")
        .Produces<NurseCertificateDto>(StatusCodes.Status200OK);

        nurseProfile.MapDelete("/certificates/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNurseCertificateCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteNurseCertificate")
        .Produces(StatusCodes.Status204NoContent);

        nurseProfile.MapGet("/languages", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseLanguagesQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseLanguages")
        .Produces<IReadOnlyList<NurseLanguageDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPut("/languages", async (UpdateNurseLanguagesCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseLanguages")
        .Produces<IReadOnlyList<NurseLanguageDto>>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/skills", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCurrentNurseSkillsQuery());
            return Results.Ok(result);
        })
        .WithName("ListCurrentNurseSkills")
        .Produces<IReadOnlyList<NurseSkillDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPut("/skills", async (UpdateNurseSkillsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateNurseSkills")
        .Produces<IReadOnlyList<NurseSkillDto>>(StatusCodes.Status200OK);

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
        .WithName("ListReceivedContactRequests")
        .Produces<PaginatedResult<ReceivedContactRequestDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/contact-requests/{id:guid}/approve", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ApproveReceivedContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("ApproveReceivedContactRequest")
        .Produces<ReceivedContactRequestDto>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/contact-requests/{id:guid}/reject", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new RejectReceivedContactRequestCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("RejectReceivedContactRequest")
        .Produces<ReceivedContactRequestDto>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/exam-analytics/summary", async (
            DateTime? from,
            DateTime? to,
            Guid? countryId,
            Guid? categoryId,
            Guid? examId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetMyExamAnalyticsSummaryQuery
            {
                From = from,
                To = to,
                CountryId = countryId,
                CategoryId = categoryId,
                ExamId = examId
            });
            return Results.Ok(result);
        })
        .WithName("GetMyExamAnalyticsSummary")
        .Produces<ExamAnalyticsSummaryDto>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/exam-analytics/by-exam", async (
            DateTime? from,
            DateTime? to,
            Guid? countryId,
            Guid? categoryId,
            Guid? examId,
            int? page,
            int? pageSize,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyExamAnalyticsByExamQuery
            {
                From = from,
                To = to,
                CountryId = countryId,
                CategoryId = categoryId,
                ExamId = examId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            });
            return Results.Ok(result);
        })
        .WithName("ListMyExamAnalyticsByExam")
        .Produces<PaginatedResult<ExamAnalyticsByExamDto>>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/exam-analytics/by-category", async (
            DateTime? from,
            DateTime? to,
            Guid? countryId,
            Guid? categoryId,
            int? page,
            int? pageSize,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyExamAnalyticsByCategoryQuery
            {
                From = from,
                To = to,
                CountryId = countryId,
                CategoryId = categoryId,
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            });
            return Results.Ok(result);
        })
        .WithName("ListMyExamAnalyticsByCategory")
        .Produces<PaginatedResult<ExamAnalyticsByCategoryDto>>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/exam-analytics/trends", async (
            DateTime? from,
            DateTime? to,
            Guid? countryId,
            Guid? categoryId,
            Guid? examId,
            string? bucket,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyExamAnalyticsTrendsQuery
            {
                From = from,
                To = to,
                CountryId = countryId,
                CategoryId = categoryId,
                ExamId = examId,
                Bucket = ParseExamAnalyticsBucket(bucket)
            });
            return Results.Ok(result);
        })
        .WithName("ListMyExamAnalyticsTrends")
        .Produces<IReadOnlyList<ExamAnalyticsTrendPointDto>>(StatusCodes.Status200OK);

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
        .WithName("ListMyExamAttempts")
        .Produces<PaginatedResult<ExamAttemptDto>>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/payment/orders", async (CreatePaymentOrderRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateMyPaymentOrderCommand { Request = request });
            return Results.Created($"/api/v1/me/nurse-profile/payment/orders/{result.Id}", result);
        })
        .WithName("CreateMyPaymentOrder")
        .Produces<PaymentOrderDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        nurseProfile.MapGet("/payment/orders", async (
            int? page,
            int? pageSize,
            PaymentOrderStatus? status,
            ISender sender) =>
        {
            var result = await sender.Send(new ListMyPaymentOrdersQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Status = status
            });
            return Results.Ok(result);
        })
        .WithName("ListMyPaymentOrders")
        .Produces<PaginatedResult<PaymentOrderDto>>(StatusCodes.Status200OK);

        nurseProfile.MapGet("/payment/orders/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetMyPaymentOrderQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("GetMyPaymentOrder")
        .Produces<PaymentOrderDto>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/payment/orders/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelMyPaymentOrderCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("CancelMyPaymentOrder")
        .Produces<PaymentOrderDto>(StatusCodes.Status200OK);

        nurseProfile.MapPost("/payment/orders/{orderId:guid}/checkout", async (
            Guid orderId,
            StartPaymentCheckoutRequest request,
            ISender sender,
            HttpContext httpContext) =>
        {
            var result = await sender.Send(new StartMyPaymentCheckoutCommand
            {
                OrderId = orderId,
                Request = request
            });

            httpContext.Response.Headers.CacheControl = "no-store";
            return Results.Ok(result);
        })
        .WithName("StartMyPaymentCheckout")
        .Produces<PaymentCheckoutSessionDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        if (app.Environment.IsDevelopment() || string.Equals(app.Environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase))
        {
            api.MapPost("/dev/sandbox/payment/checkout-sessions/{checkoutSessionId:guid}/complete", async (
                Guid checkoutSessionId,
                ISender sender,
                HttpContext httpContext) =>
            {
                var result = await sender.Send(new CompleteSandboxPaymentCheckoutCommand
                {
                    CheckoutSessionId = checkoutSessionId
                });

                httpContext.Response.Headers.CacheControl = "no-store";
                return Results.Ok(result);
            })
            .WithName("CompleteSandboxPaymentCheckout")
            .Produces<PaymentCompletionDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .RequireAuthorization();
        }

        nurseProfile.MapGet("/cv", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCurrentNurseCvQuery());
            return Results.Ok(result);
        })
        .WithName("GetCurrentNurseCv")
        .Produces<NurseCvDocumentDto>(StatusCodes.Status200OK);

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
        .WithName("UploadNurseCv")
        .Produces<NurseCvDocumentDto>(StatusCodes.Status200OK)
        .AddOpenApiOperationTransformer((operation, _, _) =>
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Required = new HashSet<string>(StringComparer.Ordinal) { "file" },
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Format = "binary"
                                }
                            }
                        }
                    }
                }
            };

            return Task.CompletedTask;
        });

        nurseProfile.MapDelete("/cv", async (ISender sender) =>
        {
            await sender.Send(new DeleteNurseCvCommand());
            return Results.NoContent();
        })
        .WithName("DeleteNurseCv")
        .Produces(StatusCodes.Status204NoContent);

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

    private static ExamAnalyticsBucket ParseExamAnalyticsBucket(string? bucket)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return ExamAnalyticsBucket.Month;
        }

        return Enum.TryParse<ExamAnalyticsBucket>(bucket, ignoreCase: true, out var parsed)
            ? parsed
            : (ExamAnalyticsBucket)(-1);
    }
}
