using MediatR;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.PreparationPackages.Admin.PackageDefinitions;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.Catalog;
using NursingPlatform.Application.PreparationPackages.DTOs;
using NursingPlatform.Application.PreparationPackages.Entitlements.DTOs;
using NursingPlatform.Application.PreparationPackages.Entitlements.GetMyPackageEntitlement;
using NursingPlatform.Application.PreparationPackages.Entitlements.ListMyPackageEntitlements;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;
using NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;

namespace NursingPlatform.WebApi.Extensions;

public static class PreparationPackageEndpointExtensions
{
    public static RouteGroupBuilder MapPreparationPackageEndpoints(this RouteGroupBuilder api)
    {
        MapPublicCatalogEndpoints(api);
        MapNurseEntitlementEndpoints(api);
        MapAdminEndpoints(api);

        return api;
    }

    private static void MapNurseEntitlementEndpoints(RouteGroupBuilder api)
    {
        var entitlements = api.MapGroup("/me/nurse-profile/preparation-packages/entitlements");

        entitlements.MapGet("/", async (int? page, int? pageSize, ISender sender) =>
        {
            var result = await sender.Send(new ListMyPackageEntitlementsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            });

            return Results.Ok(result);
        })
        .WithName("ListMyPackageEntitlements")
        .WithNurseEntitlementMetadata<PaginatedResult<PackageEntitlementListItemDto>>(
            "List my preparation package entitlements",
            "Lists preparation package entitlements owned by the current nurse profile.")
        .RequireAuthorization();

        entitlements.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetMyPackageEntitlementQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("GetMyPackageEntitlement")
        .WithNurseEntitlementMetadata<PackageEntitlementDetailDto>(
            "Get my preparation package entitlement",
            "Gets one preparation package entitlement owned by the current nurse profile.",
            includeNotFound: true)
        .RequireAuthorization();

        entitlements.MapPost("/{entitlementId:guid}/exam-session", async (Guid entitlementId, ISender sender) =>
        {
            var result = await sender.Send(new StartPackageExamSessionCommand(entitlementId));
            return Results.Ok(result);
        })
        .WithName("StartPackageExamSession")
        .WithNurseEntitlementMetadata<PackageExamSessionStartDto>(
            "Start package exam session",
            "Starts the exam attempt for one owned preparation package entitlement.",
            includeNotFound: true,
            includeConflict: true)
        .RequireAuthorization();
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
        .WithCatalogMetadata<PaginatedResult<PreparationPackageOfferListItemDto>>(
            "List preparation package offers",
            "Lists active, catalog-eligible preparation package offers. Allows anonymous access.")
        .AllowAnonymous();

        catalog.MapGet("/{slug}", async (string slug, ISender sender) =>
        {
            var result = await sender.Send(new GetPreparationPackageOfferQuery { Slug = slug });
            return Results.Ok(result);
        })
        .WithName("GetPreparationPackageOffer")
        .WithCatalogMetadata<PreparationPackageOfferDetailDto>(
            "Get a preparation package offer",
            "Gets one active, catalog-eligible preparation package offer by slug. Allows anonymous access.",
            includeNotFound: true)
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
        .WithAdminMetadata<PaginatedResult<AdminReportingTopicDto>>(
            "Preparation Package Admin - Reporting Topics",
            "List reporting topics",
            "Lists preparation package reporting topics. Requires ReportingTopics.Manage.")
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPost("/reporting-topics", async (CreateAdminReportingTopicRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminReportingTopicCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/reporting-topics/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageReportingTopic")
        .WithAdminCreatedMetadata<AdminReportingTopicDto>(
            "Preparation Package Admin - Reporting Topics",
            "Create a reporting topic",
            "Creates a preparation package reporting topic. Requires ReportingTopics.Manage.")
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPut("/reporting-topics/{id:guid}", async (Guid id, UpdateAdminReportingTopicRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminReportingTopicCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageReportingTopic")
        .WithAdminMetadata<AdminReportingTopicDto>(
            "Preparation Package Admin - Reporting Topics",
            "Update a reporting topic",
            "Updates a preparation package reporting topic. Requires ReportingTopics.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.ReportingTopics.Manage);

        admin.MapPost("/reporting-topics/{id:guid}/archive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveAdminReportingTopicCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminArchivePreparationPackageReportingTopic")
        .WithAdminMetadata<AdminReportingTopicDto>(
            "Preparation Package Admin - Reporting Topics",
            "Archive a reporting topic",
            "Archives a preparation package reporting topic. Requires ReportingTopics.Manage.",
            includeNotFound: true,
            includeConflict: true)
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
        .WithAdminMetadata<PaginatedResult<AdminReportingProfilePublicationDto>>(
            "Preparation Package Admin - Reporting Profiles",
            "List reporting profiles",
            "Lists preparation package reporting profile publications. Requires ReportingProfiles.Manage.")
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapPost("/reporting-profiles", async (CreateAdminReportingProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminReportingProfileCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/reporting-profiles/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageReportingProfile")
        .WithAdminCreatedMetadata<AdminReportingProfilePublicationDto>(
            "Preparation Package Admin - Reporting Profiles",
            "Create a reporting profile",
            "Creates a preparation package reporting profile publication draft. Requires ReportingProfiles.Manage.")
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapGet("/reporting-profiles/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminReportingProfileQuery { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminGetPreparationPackageReportingProfile")
        .WithAdminMetadata<AdminReportingProfilePublicationDto>(
            "Preparation Package Admin - Reporting Profiles",
            "Get a reporting profile",
            "Gets a preparation package reporting profile publication. Requires ReportingProfiles.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.ReportingProfiles.Manage);

        admin.MapPost("/reporting-profiles/{id:guid}/publish", async (Guid id, PublishAdminReportingProfileRequest request, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminReportingProfileCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageReportingProfile")
        .WithAdminMetadata<AdminReportingProfilePublicationDto>(
            "Preparation Package Admin - Reporting Profiles",
            "Publish a reporting profile",
            "Publishes a preparation package reporting profile. Requires ReportingProfiles.Manage.",
            includeNotFound: true,
            includeConflict: true)
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
        .WithAdminMetadata<PaginatedResult<AdminStudyMaterialDto>>(
            "Preparation Package Admin - Study Materials",
            "List study materials",
            "Lists preparation package study materials. Requires StudyMaterials.Manage.")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials", async (CreateAdminStudyMaterialRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminStudyMaterialCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/materials/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageStudyMaterial")
        .WithAdminCreatedMetadata<AdminStudyMaterialDto>(
            "Preparation Package Admin - Study Materials",
            "Create a study material",
            "Creates a preparation package study material. Requires StudyMaterials.Manage.")
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions", async (Guid materialId, CreateAdminStudyMaterialVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/materials/{materialId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageStudyMaterialVersion")
        .WithAdminCreatedMetadata<AdminStudyMaterialVersionDto>(
            "Preparation Package Admin - Study Materials",
            "Create a study material version",
            "Creates a preparation package study material version. Requires StudyMaterials.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPut("/materials/{materialId:guid}/versions/{versionId:guid}", async (Guid materialId, Guid versionId, UpdateAdminStudyMaterialVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageStudyMaterialVersion")
        .WithAdminMetadata<AdminStudyMaterialVersionDto>(
            "Preparation Package Admin - Study Materials",
            "Update a study material version",
            "Updates a preparation package study material version. Requires StudyMaterials.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions/{versionId:guid}/publish", async (Guid materialId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageStudyMaterialVersion")
        .WithAdminMetadata<AdminStudyMaterialVersionDto>(
            "Preparation Package Admin - Study Materials",
            "Publish a study material version",
            "Publishes a preparation package study material version. Requires StudyMaterials.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.StudyMaterials.Manage);

        admin.MapPost("/materials/{materialId:guid}/versions/{versionId:guid}/retire", async (Guid materialId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminStudyMaterialVersionCommand { StudyMaterialId = materialId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackageStudyMaterialVersion")
        .WithAdminMetadata<AdminStudyMaterialVersionDto>(
            "Preparation Package Admin - Study Materials",
            "Retire a study material version",
            "Retires a preparation package study material version. Requires StudyMaterials.Manage.",
            includeNotFound: true,
            includeConflict: true)
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
        .WithAdminMetadata<PaginatedResult<AdminPracticeCollectionDto>>(
            "Preparation Package Admin - Practice Collections",
            "List practice collections",
            "Lists preparation package practice collections. Requires PracticeCollections.Manage.")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections", async (CreateAdminPracticeCollectionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPracticeCollectionCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/practice-collections/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackagePracticeCollection")
        .WithAdminCreatedMetadata<AdminPracticeCollectionDto>(
            "Preparation Package Admin - Practice Collections",
            "Create a practice collection",
            "Creates a preparation package practice collection. Requires PracticeCollections.Manage.")
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions", async (Guid collectionId, CreateAdminPracticeCollectionVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/practice-collections/{collectionId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackagePracticeCollectionVersion")
        .WithAdminCreatedMetadata<AdminPracticeCollectionVersionDto>(
            "Preparation Package Admin - Practice Collections",
            "Create a practice collection version",
            "Creates a preparation package practice collection version. Requires PracticeCollections.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPut("/practice-collections/{collectionId:guid}/versions/{versionId:guid}", async (Guid collectionId, Guid versionId, UpdateAdminPracticeCollectionVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackagePracticeCollectionVersion")
        .WithAdminMetadata<AdminPracticeCollectionVersionDto>(
            "Preparation Package Admin - Practice Collections",
            "Update a practice collection version",
            "Updates a preparation package practice collection version. Requires PracticeCollections.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions/{versionId:guid}/publish", async (Guid collectionId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackagePracticeCollectionVersion")
        .WithAdminMetadata<AdminPracticeCollectionVersionDto>(
            "Preparation Package Admin - Practice Collections",
            "Publish a practice collection version",
            "Publishes a preparation package practice collection version. Requires PracticeCollections.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.PracticeCollections.Manage);

        admin.MapPost("/practice-collections/{collectionId:guid}/versions/{versionId:guid}/retire", async (Guid collectionId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminPracticeCollectionVersionCommand { PracticeCollectionId = collectionId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackagePracticeCollectionVersion")
        .WithAdminMetadata<AdminPracticeCollectionVersionDto>(
            "Preparation Package Admin - Practice Collections",
            "Retire a practice collection version",
            "Retires a preparation package practice collection version. Requires PracticeCollections.Manage.",
            includeNotFound: true,
            includeConflict: true)
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
        .WithAdminMetadata<PaginatedResult<AdminPreparationPackageDefinitionDto>>(
            "Preparation Package Admin - Packages",
            "List preparation package definitions",
            "Lists preparation package definitions. Requires PreparationPackages.View.")
        .RequirePermission(Permissions.PreparationPackages.View);

        admin.MapPost("/packages", async (CreateAdminPreparationPackageDefinitionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageDefinitionCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/packages/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageDefinition")
        .WithAdminCreatedMetadata<AdminPreparationPackageDefinitionDto>(
            "Preparation Package Admin - Packages",
            "Create a preparation package definition",
            "Creates a preparation package definition. Requires PreparationPackages.Manage.")
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapPut("/packages/{id:guid}", async (Guid id, UpdateAdminPreparationPackageDefinitionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPreparationPackageDefinitionCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageDefinition")
        .WithAdminMetadata<AdminPreparationPackageDefinitionDto>(
            "Preparation Package Admin - Packages",
            "Update a preparation package definition",
            "Updates a preparation package definition. Requires PreparationPackages.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapPost("/packages/{packageId:guid}/versions", async (Guid packageId, CreateAdminPreparationPackageVersionRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/packages/{packageId}/versions/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageVersion")
        .WithAdminCreatedMetadata<AdminPreparationPackageVersionDto>(
            "Preparation Package Admin - Packages",
            "Create a preparation package version",
            "Creates a preparation package version. Requires PreparationPackages.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.PreparationPackages.Manage);

        admin.MapGet("/packages/{packageId:guid}/versions/{versionId:guid}/validation", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new GetAdminPreparationPackageVersionValidationQuery { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminValidatePreparationPackageVersion")
        .WithAdminMetadata<PackagePublicationValidationDto>(
            "Preparation Package Admin - Packages",
            "Validate a preparation package version",
            "Validates preparation package version publication readiness. Requires PreparationPackages.View.",
            includeNotFound: true)
        .RequirePermission(Permissions.PreparationPackages.View);

        admin.MapPost("/packages/{packageId:guid}/versions/{versionId:guid}/publish", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new PublishAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminPublishPreparationPackageVersion")
        .WithAdminMetadata<AdminPreparationPackageVersionDto>(
            "Preparation Package Admin - Packages",
            "Publish a preparation package version",
            "Publishes a preparation package version. Requires PreparationPackages.Publish.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.PreparationPackages.Publish);

        admin.MapPost("/packages/{packageId:guid}/versions/{versionId:guid}/retire", async (Guid packageId, Guid versionId, ISender sender) =>
        {
            var result = await sender.Send(new RetireAdminPreparationPackageVersionCommand { PreparationPackageDefinitionId = packageId, VersionId = versionId });
            return Results.Ok(result);
        })
        .WithName("AdminRetirePreparationPackageVersion")
        .WithAdminMetadata<AdminPreparationPackageVersionDto>(
            "Preparation Package Admin - Packages",
            "Retire a preparation package version",
            "Retires a preparation package version. Requires PreparationPackages.Publish.",
            includeNotFound: true,
            includeConflict: true)
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
        .WithAdminMetadata<PaginatedResult<AdminPreparationPackageOfferDto>>(
            "Preparation Package Admin - Offers",
            "List preparation package offers",
            "Lists preparation package offers for administration. Requires PreparationPackageOffers.Manage.")
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers", async (CreateAdminPreparationPackageOfferRequest request, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdminPreparationPackageOfferCommand { Request = request });
            return Results.Created($"/api/v1/admin/preparation-package/offers/{result.Id}", result);
        })
        .WithName("AdminCreatePreparationPackageOffer")
        .WithAdminCreatedMetadata<AdminPreparationPackageOfferDto>(
            "Preparation Package Admin - Offers",
            "Create a preparation package offer",
            "Creates a preparation package offer. Requires PreparationPackageOffers.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPut("/offers/{id:guid}", async (Guid id, UpdateAdminPreparationPackageOfferRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdminPreparationPackageOfferCommand { Id = id, Request = request });
            return Results.Ok(result);
        })
        .WithName("AdminUpdatePreparationPackageOffer")
        .WithAdminMetadata<AdminPreparationPackageOfferDto>(
            "Preparation Package Admin - Offers",
            "Update a preparation package offer",
            "Updates a preparation package offer. Requires PreparationPackageOffers.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers/{id:guid}/activate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ActivateAdminPreparationPackageOfferCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminActivatePreparationPackageOffer")
        .WithAdminMetadata<AdminPreparationPackageOfferDto>(
            "Preparation Package Admin - Offers",
            "Activate a preparation package offer",
            "Activates a preparation package offer. Requires PreparationPackageOffers.Manage.",
            includeNotFound: true,
            includeConflict: true)
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);

        admin.MapPost("/offers/{id:guid}/deactivate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeactivateAdminPreparationPackageOfferCommand { Id = id });
            return Results.Ok(result);
        })
        .WithName("AdminDeactivatePreparationPackageOffer")
        .WithAdminMetadata<AdminPreparationPackageOfferDto>(
            "Preparation Package Admin - Offers",
            "Deactivate a preparation package offer",
            "Deactivates a preparation package offer. Requires PreparationPackageOffers.Manage.",
            includeNotFound: true)
        .RequirePermission(Permissions.PreparationPackageOffers.Manage);
    }

    private static RouteHandlerBuilder WithCatalogMetadata<TResponse>(
        this RouteHandlerBuilder builder,
        string summary,
        string description,
        bool includeNotFound = false)
    {
        builder
            .WithTags("Preparation Package Catalog")
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<TResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        if (includeNotFound)
        {
            builder.ProducesProblem(StatusCodes.Status404NotFound);
        }

        return builder;
    }

    private static RouteHandlerBuilder WithNurseEntitlementMetadata<TResponse>(
        this RouteHandlerBuilder builder,
        string summary,
        string description,
        bool includeNotFound = false,
        bool includeConflict = false)
    {
        builder
            .WithTags("Preparation Package Entitlements")
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<TResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        if (includeNotFound)
        {
            builder.ProducesProblem(StatusCodes.Status404NotFound);
        }

        if (includeConflict)
        {
            builder.ProducesProblem(StatusCodes.Status409Conflict);
        }

        return builder;
    }

    private static RouteHandlerBuilder WithAdminMetadata<TResponse>(
        this RouteHandlerBuilder builder,
        string tag,
        string summary,
        string description,
        bool includeNotFound = false,
        bool includeConflict = false)
    {
        builder
            .WithTags(tag)
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<TResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        if (includeNotFound)
        {
            builder.ProducesProblem(StatusCodes.Status404NotFound);
        }

        if (includeConflict)
        {
            builder.ProducesProblem(StatusCodes.Status409Conflict);
        }

        return builder;
    }

    private static RouteHandlerBuilder WithAdminCreatedMetadata<TResponse>(
        this RouteHandlerBuilder builder,
        string tag,
        string summary,
        string description,
        bool includeNotFound = false)
    {
        builder
            .WithTags(tag)
            .WithSummary(summary)
            .WithDescription(description)
            .Produces<TResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        if (includeNotFound)
        {
            builder.ProducesProblem(StatusCodes.Status404NotFound);
        }

        return builder;
    }
}
