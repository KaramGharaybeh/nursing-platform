using MediatR;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.PreparationPackages.Admin.PackageDefinitions;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.Catalog;

namespace NursingPlatform.WebApi.Extensions;

public static class PreparationPackageEndpointExtensions
{
    public static RouteGroupBuilder MapPreparationPackageEndpoints(this RouteGroupBuilder api)
    {
        MapPublicCatalogEndpoints(api);
        MapAdminEndpoints(api);

        return api;
    }

    private static void MapPublicCatalogEndpoints(RouteGroupBuilder api)
    {
        var catalog = api.MapGroup("/preparation-packages/offers");

        catalog.MapGet("/", async (
            int? page,
            int? pageSize,
            Guid? countryId,
            Guid? examCategoryId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListPreparationPackageOffersQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                CountryId = countryId,
                ExamCategoryId = examCategoryId
            });

            return Results.Ok(result);
        })
        .WithName("ListPreparationPackageOffers")
        .AllowAnonymous();

        catalog.MapGet("/{slug}", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new GetPreparationPackageOfferQuery { Slug = slug });
            return Results.Ok(result);
        })
        .WithName("GetPreparationPackageOffer")
        .AllowAnonymous();
    }

    private static void MapAdminEndpoints(RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin/preparation-package");

        MapReportingTopicEndpoints(admin);
        MapReportingProfileEndpoints(admin);
        MapStudyMaterialEndpoints(admin);
        MapPracticeCollectionEndpoints(admin);
        MapPackageDefinitionAndVersionEndpoints(admin);
        MapOfferEndpoints(admin);
    }

    private static void MapReportingTopicEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/reporting-topics", async (
            int? page,
            int? pageSize,
            Guid? examCategoryId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminReportingTopicsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                ExamCategoryId = examCategoryId
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackageReportingTopics")
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPost("/reporting-topics", async (CreateAdminReportingTopicRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminReportingTopicCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/reporting-topics/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageReportingTopic")
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPut("/reporting-topics/{id:guid}", async (Guid id, UpdateAdminReportingTopicRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminReportingTopicCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageReportingTopic")
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPost("/reporting-topics/{id:guid}/archive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveAdminReportingTopicCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminArchivePreparationPackageReportingTopic")
        .RequirePermission(Permissions.ReportingTopics.Manage);
    }

    private static void MapReportingProfileEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/reporting-profiles", async (
            int? page,
            int? pageSize,
            Guid? examVersionId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminReportingProfilesQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                ExamVersionId = examVersionId
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackageReportingProfiles")
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapPost("/reporting-profiles", async (CreateAdminReportingProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminReportingProfileCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/reporting-profiles/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageReportingProfile")
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapGet("/reporting-profiles/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminReportingProfileQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminGetPreparationPackageReportingProfile")
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapPost("/reporting-profiles/{id:guid}/publish", async (Guid id, PublishAdminReportingProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminReportingProfileCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageReportingProfile")
        .RequirePermission(Permissions.ReportingProfiles.Manage);
    }

    private static void MapStudyMaterialEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/materials", async (int? page, int? pageSize, ISender sender) =>
        {
            var result = await sender.Send(new ListAdminStudyMaterialsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackageStudyMaterials")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials", async (CreateAdminStudyMaterialRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminStudyMaterialCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/materials/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageStudyMaterial")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions", async (Guid materialId, CreateAdminStudyMaterialVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/materials/{materialId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageStudyMaterialVersion")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPut("/materials/{materialId:guid}/versions/{versionId:guid}", async (Guid materialId, Guid versionId, UpdateAdminStudyMaterialVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageStudyMaterialVersion")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions/{versionId:guid}/publish", async (Guid materialId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageStudyMaterialVersion")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions/{versionId:guid}/retire", async (Guid materialId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackageStudyMaterialVersion")
        .RequirePermission(Permissions.StudyMaterials.Manage);
    }

    private static void MapPracticeCollectionEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/practice-collections", async (int? page, int? pageSize, ISender sender) =>
        {
            var result = await sender.Send(new ListAdminPracticeCollectionsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackagePracticeCollections")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections", async (CreateAdminPracticeCollectionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPracticeCollectionCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/practice-collections/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackagePracticeCollection")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions", async (Guid collectionId, CreateAdminPracticeCollectionVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/practice-collections/{collectionId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackagePracticeCollectionVersion")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPut("/practice-collections/{collectionId:guid}/versions/{versionId:guid}", async (Guid collectionId, Guid versionId, UpdateAdminPracticeCollectionVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackagePracticeCollectionVersion")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions/{versionId:guid}/publish", async (Guid collectionId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackagePracticeCollectionVersion")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions/{versionId:guid}/retire", async (Guid collectionId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackagePracticeCollectionVersion")
        .RequirePermission(Permissions.PracticeCollections.Manage);
    }

    private static void MapPackageDefinitionAndVersionEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/packages", async (
            int? page,
            int? pageSize,
            Guid? countryId,
            Guid? examCategoryId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminPreparationPackageDefinitionsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                CountryId = countryId,
                ExamCategoryId = examCategoryId
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackageDefinitions")
        .RequirePermission(Permissions.PreparationPackages.View);

        admin.MapPost("/packages", async (CreateAdminPreparationPackageDefinitionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageDefinitionCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/packages/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageDefinition")
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapPut("/packages/{id:guid}", async (Guid id, UpdateAdminPreparationPackageDefinitionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPreparationPackageDefinitionCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageDefinition")
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapPost("/packages/{packageId:guid}/versions", async (Guid packageId, CreateAdminPreparationPackageVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/packages/{packageId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageVersion")
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapGet("/packages/{packageId:guid}/versions/{versionId:guid}/validation", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminPreparationPackageVersionValidationQuery { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminValidatePreparationPackageVersion")
        .RequirePermission(Permissions.PreparationPackages.View);

        admin.MapPost("/packages/{packageId:guid}/versions/{versionId:guid}/publish", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageVersion")
        .RequirePermission(Permissions.PreparationPackages.Publish);

        admin.MapPost("/packages/{packageId:guid}/versions/{versionId:guid}/retire", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackageVersion")
        .RequirePermission(Permissions.PreparationPackages.Publish);
    }

    private static void MapOfferEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/offers", async (
            int? page,
            int? pageSize,
            Guid? preparationPackageDefinitionId,
            ISender sender) =>
        {
            var result = await sender.Send(new ListAdminPreparationPackageOffersQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                PreparationPackageDefinitionId = preparationPackageDefinitionId
            });

            return Results.Ok(result);
        })
        .WithName("AdminListPreparationPackageOffers")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers", async (CreateAdminPreparationPackageOfferRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageOfferCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/offers/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageOffer")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPut("/offers/{id:guid}", async (Guid id, UpdateAdminPreparationPackageOfferRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPreparationPackageOfferCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageOffer")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers/{id:guid}/activate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ActivateAdminPreparationPackageOfferCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminActivatePreparationPackageOffer")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers/{id:guid}/deactivate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeactivateAdminPreparationPackageOfferCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminDeactivatePreparationPackageOffer")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);
    }
}
