using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public sealed class PackageAnalyticalReportConfiguration : IEntityTypeConfiguration<PackageAnalyticalReport>
{
    public void Configure(EntityTypeBuilder<PackageAnalyticalReport> builder)
    {
        builder.ToTable("PackageAnalyticalReports");
        builder.HasKey(report => report.Id);

        builder.HasIndex(report => report.ExamSessionId).IsUnique();
        builder.HasIndex(report => new
        {
            report.NurseProfileId,
            report.PackagePurchaseEntitlementId,
            report.ExamSessionId
        });
        builder.HasIndex(report => report.ExamSessionProvenanceId).IsUnique();

        builder.Property(report => report.NurseProfileId).IsRequired();
        builder.Property(report => report.ExamSessionId).IsRequired();
        builder.Property(report => report.ExamSessionProvenanceId).IsRequired();
        builder.Property(report => report.PackagePurchaseEntitlementId).IsRequired();
        builder.Property(report => report.PackageOrderItemSnapshotId).IsRequired();
        builder.Property(report => report.PaymentOrderId).IsRequired();
        builder.Property(report => report.PaymentOrderItemId).IsRequired();
        builder.Property(report => report.PreparationPackageDefinitionId).IsRequired();
        builder.Property(report => report.PreparationPackageVersionId).IsRequired();
        builder.Property(report => report.PreparationPackageOfferId).IsRequired();
        builder.Property(report => report.IncludedExamId).IsRequired();
        builder.Property(report => report.IncludedExamVersionId).IsRequired();
        builder.Property(report => report.ReportingProfilePublicationId).IsRequired();
        builder.Property(report => report.PracticeCollectionVersionId).IsRequired();
        builder.Property(report => report.GeneratedAt).IsRequired();
        builder.Property(report => report.FinalizedSessionStatus).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(report => report.SubmittedAt);
        builder.Property(report => report.FinalizedAt).IsRequired();
        builder.Property(report => report.Score).IsRequired();
        builder.Property(report => report.MaxScore).IsRequired();
        builder.Property(report => report.Percentage).HasPrecision(5, 2).IsRequired();
        builder.Property(report => report.Passed).IsRequired();
        builder.Property(report => report.CorrectCount).IsRequired();
        builder.Property(report => report.QuestionCount).IsRequired();
        builder.Property(report => report.PackageAccessStartsAt).IsRequired();
        builder.Property(report => report.PackageAccessEndsAt).IsRequired();

        builder.HasOne<NursingPlatform.Domain.Nurses.NurseProfile>()
            .WithMany()
            .HasForeignKey(report => report.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ExamSession>()
            .WithMany()
            .HasForeignKey(report => report.ExamSessionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ExamSessionProvenance>()
            .WithMany()
            .HasForeignKey(report => report.ExamSessionProvenanceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PackagePurchaseEntitlement>()
            .WithMany()
            .HasForeignKey(report => report.PackagePurchaseEntitlementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(report => report.TopicResults)
            .WithOne()
            .HasForeignKey(topic => topic.PackageAnalyticalReportId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(report => report.GuidanceItems)
            .WithOne()
            .HasForeignKey(item => item.PackageAnalyticalReportId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(report => report.TopicResults).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(report => report.GuidanceItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PackageAnalyticalReportTopicResultConfiguration : IEntityTypeConfiguration<PackageAnalyticalReportTopicResult>
{
    public void Configure(EntityTypeBuilder<PackageAnalyticalReportTopicResult> builder)
    {
        builder.ToTable("PackageAnalyticalReportTopicResults");
        builder.HasKey(topic => topic.Id);
        builder.HasIndex(topic => new { topic.PackageAnalyticalReportId, topic.SortOrder }).IsUnique();
        builder.HasIndex(topic => new { topic.PackageAnalyticalReportId, topic.ReportingTopicId }).IsUnique();

        builder.Property(topic => topic.PackageAnalyticalReportId).IsRequired();
        builder.Property(topic => topic.ReportingTopicId).IsRequired();
        builder.Property(topic => topic.TopicNameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(topic => topic.TopicDescriptionSnapshot).HasMaxLength(2000);
        builder.Property(topic => topic.ScoredQuestionCount).IsRequired();
        builder.Property(topic => topic.CorrectCount).IsRequired();
        builder.Property(topic => topic.EarnedPoints).IsRequired();
        builder.Property(topic => topic.AvailablePoints).IsRequired();
        builder.Property(topic => topic.Percentage).HasPrecision(5, 2).IsRequired();
        builder.Property(topic => topic.SortOrder).IsRequired();

        builder.HasOne<ReportingTopic>()
            .WithMany()
            .HasForeignKey(topic => topic.ReportingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PackageAnalyticalReportGuidanceItemConfiguration : IEntityTypeConfiguration<PackageAnalyticalReportGuidanceItem>
{
    public void Configure(EntityTypeBuilder<PackageAnalyticalReportGuidanceItem> builder)
    {
        builder.ToTable("PackageAnalyticalReportGuidanceItems");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.PackageAnalyticalReportId, item.SortOrder }).IsUnique();
        builder.HasIndex(item => new { item.PackageAnalyticalReportId, item.ReportingTopicId, item.SortOrder });

        builder.Property(item => item.PackageAnalyticalReportId).IsRequired();
        builder.Property(item => item.ReportingTopicId).IsRequired();
        builder.Property(item => item.SourceType).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(item => item.SourceVersionId).IsRequired();
        builder.Property(item => item.TitleSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(item => item.SourceMetadataSnapshot).HasMaxLength(200);
        builder.Property(item => item.SortOrder).IsRequired();

        builder.HasOne<ReportingTopic>()
            .WithMany()
            .HasForeignKey(item => item.ReportingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
