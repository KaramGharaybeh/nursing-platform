using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.ToTable("ContactRequests");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.EmployerProfileId, r.CreatedAt, r.Id });
        builder.HasIndex(r => new { r.NurseProfileId, r.CreatedAt, r.Id });
        builder.HasIndex(r => new { r.EmployerProfileId, r.NurseProfileId })
            .IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Approved')");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(r => r.CandidateHeadlineSnapshot).HasMaxLength(160);
        builder.Property(r => r.CandidateLicenseCountryNameSnapshot).HasMaxLength(200);
        builder.Property(r => r.CandidateCurrentCountryNameSnapshot).HasMaxLength(200);
        builder.Property(r => r.EmployerOrganizationNameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(r => r.JobTitleSnapshot).HasMaxLength(160);
        builder.Property(r => r.DepartmentSnapshot).HasMaxLength(160);

        builder.HasOne(r => r.EmployerProfile)
            .WithMany()
            .HasForeignKey(r => r.EmployerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.EmployerOrganization)
            .WithMany()
            .HasForeignKey(r => r.EmployerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.NurseProfile)
            .WithMany()
            .HasForeignKey(r => r.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
