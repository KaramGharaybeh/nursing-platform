using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class PreparationPackageDefinitionConfiguration : IEntityTypeConfiguration<PreparationPackageDefinition>
{
    public void Configure(EntityTypeBuilder<PreparationPackageDefinition> builder)
    {
        builder.ToTable("PreparationPackageDefinitions");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.CountryId, p.ExamCategoryId, p.Title }).IsUnique();
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.HasOne<NursingPlatform.Domain.ReferenceData.Country>()
            .WithMany()
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<NursingPlatform.Domain.Exams.ExamCategory>()
            .WithMany()
            .HasForeignKey(p => p.ExamCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PreparationPackageVersionConfiguration : IEntityTypeConfiguration<PreparationPackageVersion>
{
    public void Configure(EntityTypeBuilder<PreparationPackageVersion> builder)
    {
        builder.ToTable("PreparationPackageVersions");
        builder.HasKey(v => v.Id);
        builder.Property<int>("VersionNumber").IsRequired();
        builder.HasIndex(nameof(PreparationPackageVersion.PreparationPackageDefinitionId), "VersionNumber").IsUnique();
        builder.Property(v => v.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne<PreparationPackageDefinition>()
            .WithMany()
            .HasForeignKey(v => v.PreparationPackageDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<NursingPlatform.Domain.Exams.ExamVersion>()
            .WithMany()
            .HasForeignKey(v => v.ExamVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ReportingProfilePublication>()
            .WithMany()
            .HasForeignKey(v => v.ReportingProfilePublicationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PracticeCollectionVersion>()
            .WithMany()
            .HasForeignKey(v => v.PracticeCollectionVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Materials)
            .WithOne()
            .HasForeignKey(m => m.PreparationPackageVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(v => v.Materials).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PreparationPackageVersionMaterialConfiguration : IEntityTypeConfiguration<PreparationPackageVersionMaterial>
{
    public void Configure(EntityTypeBuilder<PreparationPackageVersionMaterial> builder)
    {
        builder.ToTable("PreparationPackageVersionMaterials");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.PreparationPackageVersionId, m.SortOrder }).IsUnique();
        builder.HasIndex(m => new { m.PreparationPackageVersionId, m.StudyMaterialVersionId }).IsUnique();
        builder.HasOne<StudyMaterialVersion>()
            .WithMany()
            .HasForeignKey(m => m.StudyMaterialVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PreparationPackageOfferConfiguration : IEntityTypeConfiguration<PreparationPackageOffer>
{
    public void Configure(EntityTypeBuilder<PreparationPackageOffer> builder)
    {
        builder.ToTable("PreparationPackageOffers");
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => o.Slug).IsUnique();
        builder.HasIndex(o => o.PreparationPackageDefinitionId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");
        builder.Property(o => o.Title).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Slug).IsRequired().HasMaxLength(160);
        builder.Property(o => o.Summary).HasMaxLength(2000);
        builder.Property(o => o.PriceAmountMinor).IsRequired();
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.Property(o => o.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne<PreparationPackageDefinition>()
            .WithMany()
            .HasForeignKey(o => o.PreparationPackageDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PreparationPackageVersion>()
            .WithMany()
            .HasForeignKey(o => o.PreparationPackageVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StudyMaterialConfiguration : IEntityTypeConfiguration<StudyMaterial>
{
    public void Configure(EntityTypeBuilder<StudyMaterial> builder)
    {
        builder.ToTable("StudyMaterials");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.Slug).IsUnique();
        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(160);
        builder.Property(m => m.Description).HasMaxLength(2000);
    }
}

public class StudyMaterialVersionConfiguration : IEntityTypeConfiguration<StudyMaterialVersion>
{
    public void Configure(EntityTypeBuilder<StudyMaterialVersion> builder)
    {
        builder.ToTable("StudyMaterialVersions");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => new { v.StudyMaterialId, v.VersionNumber }).IsUnique();
        builder.Property(v => v.MaterialType).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(v => v.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(v => v.FormattedTextContent).HasMaxLength(20000);
        builder.Property(v => v.FileStorageKey).HasMaxLength(1024);
        builder.Property(v => v.ExternalUrl).HasMaxLength(2048);
        builder.Property(v => v.VideoUrl).HasMaxLength(2048);
        builder.HasOne<StudyMaterial>()
            .WithMany()
            .HasForeignKey(v => v.StudyMaterialId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Topics)
            .WithOne()
            .HasForeignKey(t => t.StudyMaterialVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(v => v.Topics).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class StudyMaterialVersionTopicConfiguration : IEntityTypeConfiguration<StudyMaterialVersionTopic>
{
    public void Configure(EntityTypeBuilder<StudyMaterialVersionTopic> builder)
    {
        builder.ToTable("StudyMaterialVersionTopics");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.StudyMaterialVersionId, t.ReportingTopicId }).IsUnique();
        builder.HasOne<ReportingTopic>()
            .WithMany()
            .HasForeignKey(t => t.ReportingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PracticeCollectionConfiguration : IEntityTypeConfiguration<PracticeCollection>
{
    public void Configure(EntityTypeBuilder<PracticeCollection> builder)
    {
        builder.ToTable("PracticeCollections");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Description).HasMaxLength(2000);
    }
}

public class PracticeCollectionVersionConfiguration : IEntityTypeConfiguration<PracticeCollectionVersion>
{
    public void Configure(EntityTypeBuilder<PracticeCollectionVersion> builder)
    {
        builder.ToTable("PracticeCollectionVersions");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => new { v.PracticeCollectionId, v.VersionNumber }).IsUnique();
        builder.Property(v => v.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne<PracticeCollection>()
            .WithMany()
            .HasForeignKey(v => v.PracticeCollectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.Items)
            .WithOne()
            .HasForeignKey(i => i.PracticeCollectionVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(v => v.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PracticeItemConfiguration : IEntityTypeConfiguration<PracticeItem>
{
    public void Configure(EntityTypeBuilder<PracticeItem> builder)
    {
        builder.ToTable("PracticeItems");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => new { i.PracticeCollectionVersionId, i.DisplayOrder }).IsUnique();
        builder.Property(i => i.Prompt).IsRequired().HasMaxLength(4000);
        builder.Property(i => i.ImmediateFeedback).IsRequired().HasMaxLength(4000);
        builder.HasOne<ReportingTopic>()
            .WithMany()
            .HasForeignKey(i => i.ReportingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(i => i.AnswerOptions)
            .WithOne()
            .HasForeignKey(o => o.PracticeItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(i => i.AnswerOptions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PracticeAnswerOptionConfiguration : IEntityTypeConfiguration<PracticeAnswerOption>
{
    public void Configure(EntityTypeBuilder<PracticeAnswerOption> builder)
    {
        builder.ToTable("PracticeAnswerOptions");
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.PracticeItemId, o.DisplayOrder }).IsUnique();
        builder.Property(o => o.OptionText).IsRequired().HasMaxLength(2000);
    }
}

public class ReportingTopicConfiguration : IEntityTypeConfiguration<ReportingTopic>
{
    public void Configure(EntityTypeBuilder<ReportingTopic> builder)
    {
        builder.ToTable("ReportingTopics");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.ExamCategoryId, t.Name }).IsUnique();
        builder.HasIndex(t => new { t.ExamCategoryId, t.Slug }).IsUnique();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(160);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.HasOne<NursingPlatform.Domain.Exams.ExamCategory>()
            .WithMany()
            .HasForeignKey(t => t.ExamCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReportingProfilePublicationConfiguration : IEntityTypeConfiguration<ReportingProfilePublication>
{
    public void Configure(EntityTypeBuilder<ReportingProfilePublication> builder)
    {
        builder.ToTable("ReportingProfilePublications");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.ExamVersionId, p.Name }).IsUnique();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne<NursingPlatform.Domain.Exams.ExamVersion>()
            .WithMany()
            .HasForeignKey(p => p.ExamVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Assignments)
            .WithOne()
            .HasForeignKey(a => a.ReportingProfilePublicationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(p => p.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ReportingProfileQuestionAssignmentConfiguration : IEntityTypeConfiguration<ReportingProfileQuestionAssignment>
{
    public void Configure(EntityTypeBuilder<ReportingProfileQuestionAssignment> builder)
    {
        builder.ToTable("ReportingProfileQuestionAssignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.ReportingProfilePublicationId, a.ExamQuestionId }).IsUnique();
        builder.HasOne<NursingPlatform.Domain.Exams.ExamQuestion>()
            .WithMany()
            .HasForeignKey(a => a.ExamQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ReportingTopic>()
            .WithMany()
            .HasForeignKey(a => a.ReportingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PackagePurchaseEntitlementConfiguration : IEntityTypeConfiguration<PackagePurchaseEntitlement>
{
    public void Configure(EntityTypeBuilder<PackagePurchaseEntitlement> builder)
    {
        builder.ToTable("PackagePurchaseEntitlements");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.PaymentOrderItemId).IsUnique();
        builder.HasIndex(e => e.PaymentOrderId);
        builder.HasIndex(e => new { e.NurseProfileId, e.PreparationPackageDefinitionId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");
        builder.HasIndex(e => new { e.NurseProfileId, e.Status, e.AccessEndsAt, e.Id });
        builder.HasIndex(e => new { e.PreparationPackageDefinitionId, e.PreparationPackageVersionId });
        builder.HasIndex(e => e.PreparationPackageOfferId);
        builder.HasIndex(e => e.PurchasedOfferSnapshotId).IsUnique();

        builder.Property(e => e.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(3);
        builder.Property(e => e.PriceAmountMinor).IsRequired();
        builder.Property(e => e.AccessDurationDays).IsRequired();
        builder.Property(e => e.FulfilledAt).IsRequired();
        builder.Property(e => e.AccessStartsAt).IsRequired();
        builder.Property(e => e.AccessEndsAt).IsRequired();
        builder.Property("_studyMaterialVersionIds")
            .HasColumnName("StudyMaterialVersionIds")
            .IsRequired();

        builder.HasOne<NursingPlatform.Domain.Nurses.NurseProfile>()
            .WithMany()
            .HasForeignKey(e => e.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentOrder>()
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentOrderItem>()
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PackageOrderItemSnapshot>()
            .WithMany()
            .HasForeignKey(e => e.PurchasedOfferSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PreparationPackageDefinition>()
            .WithMany()
            .HasForeignKey(e => e.PreparationPackageDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PreparationPackageVersion>()
            .WithMany()
            .HasForeignKey(e => e.PreparationPackageVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PreparationPackageOffer>()
            .WithMany()
            .HasForeignKey(e => e.PreparationPackageOfferId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<NursingPlatform.Domain.Exams.Exam>()
            .WithMany()
            .HasForeignKey(e => e.IncludedExamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<NursingPlatform.Domain.Exams.ExamVersion>()
            .WithMany()
            .HasForeignKey(e => e.IncludedExamVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ReportingProfilePublication>()
            .WithMany()
            .HasForeignKey(e => e.ReportingProfilePublicationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PracticeCollectionVersion>()
            .WithMany()
            .HasForeignKey(e => e.PracticeCollectionVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Rights)
            .WithOne()
            .HasForeignKey(r => r.PackagePurchaseEntitlementId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(e => e.Rights).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PackageBenefitRightConfiguration : IEntityTypeConfiguration<PackageBenefitRight>
{
    public void Configure(EntityTypeBuilder<PackageBenefitRight> builder)
    {
        builder.ToTable("PackageBenefitRights");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.PackagePurchaseEntitlementId, r.RightType }).IsUnique();
        builder.HasIndex(r => new { r.PackagePurchaseEntitlementId, r.RightType, r.Status });
        builder.Property(r => r.RightType).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(r => r.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(r => r.AccessStartsAt).IsRequired();
        builder.Property(r => r.AccessEndsAt);
        builder.HasOne<PackagePurchaseEntitlement>()
            .WithMany()
            .HasForeignKey(r => r.PackagePurchaseEntitlementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
