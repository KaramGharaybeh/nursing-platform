using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class PreparationPackageConfigurationTests
{
    [Theory]
    [InlineData(typeof(PreparationPackageDefinition), "PreparationPackageDefinitions")]
    [InlineData(typeof(PreparationPackageVersion), "PreparationPackageVersions")]
    [InlineData(typeof(PreparationPackageVersionMaterial), "PreparationPackageVersionMaterials")]
    [InlineData(typeof(PreparationPackageOffer), "PreparationPackageOffers")]
    [InlineData(typeof(StudyMaterial), "StudyMaterials")]
    [InlineData(typeof(StudyMaterialVersion), "StudyMaterialVersions")]
    [InlineData(typeof(StudyMaterialVersionTopic), "StudyMaterialVersionTopics")]
    [InlineData(typeof(PracticeCollection), "PracticeCollections")]
    [InlineData(typeof(PracticeCollectionVersion), "PracticeCollectionVersions")]
    [InlineData(typeof(PracticeItem), "PracticeItems")]
    [InlineData(typeof(PracticeAnswerOption), "PracticeAnswerOptions")]
    [InlineData(typeof(ReportingTopic), "ReportingTopics")]
    [InlineData(typeof(ReportingProfilePublication), "ReportingProfilePublications")]
    [InlineData(typeof(ReportingProfileQuestionAssignment), "ReportingProfileQuestionAssignments")]
    [InlineData(typeof(PackagePurchaseEntitlement), "PackagePurchaseEntitlements")]
    [InlineData(typeof(PackageBenefitRight), "PackageBenefitRights")]
    [InlineData(typeof(PackageAnalyticalReport), "PackageAnalyticalReports")]
    [InlineData(typeof(PackageAnalyticalReportTopicResult), "PackageAnalyticalReportTopicResults")]
    [InlineData(typeof(PackageAnalyticalReportGuidanceItem), "PackageAnalyticalReportGuidanceItems")]
    public void PreparationPackageEntities_AreConfiguredInModel(Type entityType, string tableName)
    {
        var entity = CreateDbContext().Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());
        Assert.Equal("Id", Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void PreparationPackageIndexes_AreConfigured()
    {
        var context = CreateDbContext();

        AssertHasIndex<PreparationPackageDefinition>(context, true,
            nameof(PreparationPackageDefinition.Slug));
        AssertHasIndex<PreparationPackageDefinition>(context, true,
            nameof(PreparationPackageDefinition.CountryId), nameof(PreparationPackageDefinition.ExamCategoryId), nameof(PreparationPackageDefinition.Title));
        AssertHasIndex<PreparationPackageVersion>(context, true,
            nameof(PreparationPackageVersion.PreparationPackageDefinitionId), "VersionNumber");
        AssertHasIndex<PreparationPackageVersionMaterial>(context, true,
            nameof(PreparationPackageVersionMaterial.PreparationPackageVersionId), nameof(PreparationPackageVersionMaterial.SortOrder));
        AssertHasIndex<PreparationPackageOffer>(context, true,
            nameof(PreparationPackageOffer.Slug));
        var activeOfferIndex = AssertHasIndex<PreparationPackageOffer>(context, true,
            nameof(PreparationPackageOffer.PreparationPackageDefinitionId));
        Assert.Equal("\"Status\" = 'Active'", activeOfferIndex.GetFilter());

        AssertHasIndex<StudyMaterial>(context, true, nameof(StudyMaterial.Slug));
        AssertHasIndex<StudyMaterialVersion>(context, true,
            nameof(StudyMaterialVersion.StudyMaterialId), nameof(StudyMaterialVersion.VersionNumber));
        AssertHasIndex<StudyMaterialVersionTopic>(context, true,
            nameof(StudyMaterialVersionTopic.StudyMaterialVersionId), nameof(StudyMaterialVersionTopic.ReportingTopicId));

        AssertHasIndex<PracticeCollection>(context, true, nameof(PracticeCollection.Slug));
        AssertHasIndex<PracticeCollectionVersion>(context, true,
            nameof(PracticeCollectionVersion.PracticeCollectionId), nameof(PracticeCollectionVersion.VersionNumber));
        AssertHasIndex<PracticeItem>(context, true,
            nameof(PracticeItem.PracticeCollectionVersionId), nameof(PracticeItem.DisplayOrder));
        AssertHasIndex<PracticeAnswerOption>(context, true,
            nameof(PracticeAnswerOption.PracticeItemId), nameof(PracticeAnswerOption.DisplayOrder));

        AssertHasIndex<ReportingTopic>(context, true,
            nameof(ReportingTopic.ExamCategoryId), nameof(ReportingTopic.Name));
        AssertHasIndex<ReportingTopic>(context, true,
            nameof(ReportingTopic.ExamCategoryId), nameof(ReportingTopic.Slug));
        AssertHasIndex<ReportingProfilePublication>(context, true,
            nameof(ReportingProfilePublication.ExamVersionId), "Name");
        AssertHasIndex<ReportingProfileQuestionAssignment>(context, true,
            nameof(ReportingProfileQuestionAssignment.ReportingProfilePublicationId), nameof(ReportingProfileQuestionAssignment.ExamQuestionId));

        AssertHasIndex<PackagePurchaseEntitlement>(context, true,
            nameof(PackagePurchaseEntitlement.PaymentOrderItemId));
        var activeEntitlementIndex = AssertHasIndex<PackagePurchaseEntitlement>(context, true,
            nameof(PackagePurchaseEntitlement.NurseProfileId), nameof(PackagePurchaseEntitlement.PreparationPackageDefinitionId));
        Assert.Equal("\"Status\" = 'Active'", activeEntitlementIndex.GetFilter());
        AssertHasIndex<PackagePurchaseEntitlement>(context, false,
            nameof(PackagePurchaseEntitlement.NurseProfileId), nameof(PackagePurchaseEntitlement.Status), nameof(PackagePurchaseEntitlement.AccessEndsAt), nameof(PackagePurchaseEntitlement.Id));
        AssertHasIndex<PackagePurchaseEntitlement>(context, false,
            nameof(PackagePurchaseEntitlement.PaymentOrderId));
        AssertHasIndex<PackagePurchaseEntitlement>(context, false,
            nameof(PackagePurchaseEntitlement.PreparationPackageDefinitionId), nameof(PackagePurchaseEntitlement.PreparationPackageVersionId));
        AssertHasIndex<PackageBenefitRight>(context, true,
            nameof(PackageBenefitRight.PackagePurchaseEntitlementId), nameof(PackageBenefitRight.RightType));
        AssertHasIndex<PackageBenefitRight>(context, false,
            nameof(PackageBenefitRight.PackagePurchaseEntitlementId), nameof(PackageBenefitRight.RightType), nameof(PackageBenefitRight.Status));
        AssertHasIndex<PackageAnalyticalReport>(context, true,
            nameof(PackageAnalyticalReport.ExamSessionId));
        AssertHasIndex<PackageAnalyticalReport>(context, false,
            nameof(PackageAnalyticalReport.NurseProfileId), nameof(PackageAnalyticalReport.PackagePurchaseEntitlementId), nameof(PackageAnalyticalReport.ExamSessionId));
        AssertHasIndex<PackageAnalyticalReportTopicResult>(context, true,
            nameof(PackageAnalyticalReportTopicResult.PackageAnalyticalReportId), nameof(PackageAnalyticalReportTopicResult.SortOrder));
        AssertHasIndex<PackageAnalyticalReportGuidanceItem>(context, true,
            nameof(PackageAnalyticalReportGuidanceItem.PackageAnalyticalReportId), nameof(PackageAnalyticalReportGuidanceItem.SortOrder));
        AssertHasIndex<PackageAnalyticalReportGuidanceItem>(context, false,
            nameof(PackageAnalyticalReportGuidanceItem.PackageAnalyticalReportId), nameof(PackageAnalyticalReportGuidanceItem.ReportingTopicId), nameof(PackageAnalyticalReportGuidanceItem.SortOrder));
    }

    [Theory]
    [InlineData(typeof(PreparationPackageVersion), nameof(PreparationPackageVersion.Status))]
    [InlineData(typeof(PreparationPackageOffer), nameof(PreparationPackageOffer.Status))]
    [InlineData(typeof(StudyMaterialVersion), nameof(StudyMaterialVersion.MaterialType))]
    [InlineData(typeof(StudyMaterialVersion), nameof(StudyMaterialVersion.Status))]
    [InlineData(typeof(PracticeCollectionVersion), nameof(PracticeCollectionVersion.Status))]
    [InlineData(typeof(ReportingProfilePublication), nameof(ReportingProfilePublication.Status))]
    [InlineData(typeof(PackagePurchaseEntitlement), nameof(PackagePurchaseEntitlement.Status))]
    [InlineData(typeof(PackageBenefitRight), nameof(PackageBenefitRight.RightType))]
    [InlineData(typeof(PackageBenefitRight), nameof(PackageBenefitRight.Status))]
    [InlineData(typeof(PackageAnalyticalReportGuidanceItem), nameof(PackageAnalyticalReportGuidanceItem.SourceType))]
    public void PreparationPackageEnums_AreStoredAsStringsWithMaxLength(Type entityType, string propertyName)
    {
        var property = CreateDbContext().Model.FindEntityType(entityType)!.FindProperty(propertyName)!;

        Assert.Equal(32, property.GetMaxLength());
        Assert.NotNull(property.GetTypeMapping().Converter);
    }

    [Fact]
    public void PreparationPackageDeleteBehaviors_AreConservative()
    {
        var entityTypes = new[]
        {
            typeof(PreparationPackageDefinition),
            typeof(PreparationPackageVersion),
            typeof(PreparationPackageVersionMaterial),
            typeof(PreparationPackageOffer),
            typeof(StudyMaterialVersion),
            typeof(StudyMaterialVersionTopic),
            typeof(PracticeCollectionVersion),
            typeof(PracticeItem),
            typeof(PracticeAnswerOption),
            typeof(ReportingTopic),
            typeof(ReportingProfilePublication),
            typeof(ReportingProfileQuestionAssignment),
            typeof(PackagePurchaseEntitlement),
            typeof(PackageBenefitRight),
            typeof(PackageAnalyticalReport),
            typeof(PackageAnalyticalReportTopicResult),
            typeof(PackageAnalyticalReportGuidanceItem)
        };

        var foreignKeys = entityTypes
            .SelectMany(t => CreateDbContext().Model.FindEntityType(t)!.GetForeignKeys())
            .ToList();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void PackageEntitlementConfiguration_UsesRestrictDeleteBehaviorForFinancialSnapshotAndRightRelationships()
    {
        var context = CreateDbContext();
        var entitlementForeignKeys = context.Model.FindEntityType(typeof(PackagePurchaseEntitlement))!.GetForeignKeys().ToList();
        var rightForeignKeys = context.Model.FindEntityType(typeof(PackageBenefitRight))!.GetForeignKeys().ToList();

        Assert.Contains(entitlementForeignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "NurseProfile"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackagePurchaseEntitlement.NurseProfileId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(entitlementForeignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "PaymentOrder"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackagePurchaseEntitlement.PaymentOrderId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(entitlementForeignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "PaymentOrderItem"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackagePurchaseEntitlement.PaymentOrderItemId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(entitlementForeignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(PackageOrderItemSnapshot)
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackagePurchaseEntitlement.PurchasedOfferSnapshotId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.All(rightForeignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void PackageEntitlementConfiguration_PersistsUtcAccessWindowAndDurationDerivedEndTime()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(PackagePurchaseEntitlement))!;

        Assert.False(entity.FindProperty(nameof(PackagePurchaseEntitlement.FulfilledAt))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(PackagePurchaseEntitlement.AccessStartsAt))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(PackagePurchaseEntitlement.AccessEndsAt))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(PackagePurchaseEntitlement.AccessDurationDays))!.IsNullable);
    }

    [Fact]
    public void PackageBenefitRightConfiguration_PersistsConsumedAt()
    {
        var property = CreateDbContext().Model.FindEntityType(typeof(PackageBenefitRight))!
            .FindProperty(nameof(PackageBenefitRight.ConsumedAt));

        Assert.NotNull(property);
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void PackageAnalyticalReportConfiguration_UsesOnlyMinimalHardRelationships()
    {
        var foreignKeys = CreateDbContext().Model.FindEntityType(typeof(PackageAnalyticalReport))!
            .GetForeignKeys()
            .ToList();

        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "NurseProfile"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.NurseProfileId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "ExamSession"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.ExamSessionId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "ExamSessionProvenance"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.ExamSessionProvenanceId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(PackagePurchaseEntitlement)
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PackagePurchaseEntitlementId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);

        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PaymentOrderId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PaymentOrderItemId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PackageOrderItemSnapshotId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PreparationPackageDefinitionId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PreparationPackageVersionId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.PreparationPackageOfferId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.IncludedExamId)]));
        Assert.DoesNotContain(foreignKeys, fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PackageAnalyticalReport.IncludedExamVersionId)]));
    }

    [Fact]
    public void PackageAnalyticalReportConfiguration_ConfiguresRequiredSnapshotFieldsAndPrecision()
    {
        var reportEntity = CreateDbContext().Model.FindEntityType(typeof(PackageAnalyticalReport))!;
        var topicEntity = CreateDbContext().Model.FindEntityType(typeof(PackageAnalyticalReportTopicResult))!;

        Assert.False(reportEntity.FindProperty(nameof(PackageAnalyticalReport.GeneratedAt))!.IsNullable);
        Assert.False(reportEntity.FindProperty(nameof(PackageAnalyticalReport.FinalizedAt))!.IsNullable);
        Assert.False(reportEntity.FindProperty(nameof(PackageAnalyticalReport.PackageAccessStartsAt))!.IsNullable);
        Assert.False(reportEntity.FindProperty(nameof(PackageAnalyticalReport.PackageAccessEndsAt))!.IsNullable);
        Assert.Equal(5, reportEntity.FindProperty(nameof(PackageAnalyticalReport.Percentage))!.GetPrecision());
        Assert.Equal(2, reportEntity.FindProperty(nameof(PackageAnalyticalReport.Percentage))!.GetScale());
        Assert.Equal(5, topicEntity.FindProperty(nameof(PackageAnalyticalReportTopicResult.Percentage))!.GetPrecision());
        Assert.Equal(2, topicEntity.FindProperty(nameof(PackageAnalyticalReportTopicResult.Percentage))!.GetScale());
        Assert.False(topicEntity.FindProperty(nameof(PackageAnalyticalReportTopicResult.SortOrder))!.IsNullable);
        Assert.False(CreateDbContext().Model.FindEntityType(typeof(PackageAnalyticalReportGuidanceItem))!
            .FindProperty(nameof(PackageAnalyticalReportGuidanceItem.SortOrder))!.IsNullable);
    }

    [Fact]
    public void PreparationPackageMigration_CanGenerateIdempotentScript()
    {
        var migrations = typeof(ApplicationDbContext).Assembly.GetTypes()
            .Where(type => type.Namespace == "NursingPlatform.Infrastructure.Persistence.Migrations")
            .Select(type => type.Name)
            .ToList();

        Assert.Contains("AddPreparationPackageStage1CatalogAuthoring", migrations);
        Assert.Contains("AddPreparationPackageStage2Persistence", migrations);
        Assert.Contains("AddPreparationPackageStage4AnalyticalReports", migrations);
        Assert.DoesNotContain(migrations, name => name.Contains("Workspace", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(migrations, name => name.Contains("ReportGeneration", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(migrations, name => name.Contains("ReportAccess", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(migrations, name => name.Contains("EmployerPackage", StringComparison.OrdinalIgnoreCase));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IIndex AssertHasIndex<TEntity>(
        ApplicationDbContext context,
        bool unique,
        params string[] propertyNames)
    {
        var index = context.Model.FindEntityType(typeof(TEntity))!.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(propertyNames));

        Assert.NotNull(index);
        Assert.Equal(unique, index.IsUnique);
        return index;
    }
}
