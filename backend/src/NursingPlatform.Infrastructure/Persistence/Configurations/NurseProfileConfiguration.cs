using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseProfileConfiguration : IEntityTypeConfiguration<NurseProfile>
{
    public void Configure(EntityTypeBuilder<NurseProfile> builder)
    {
        builder.ToTable("NurseProfiles");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.LicenseCountryId);
        builder.HasIndex(p => p.CurrentCountryId);

        builder.Property(p => p.Headline).HasMaxLength(160);
        builder.Property(p => p.ProfessionalSummary).HasMaxLength(2000);
        builder.Property(p => p.LicenseNumber).HasMaxLength(100);
        builder.Property(p => p.YearsOfExperience).IsRequired();
        builder.Property(p => p.IsAvailableForRecruitment).IsRequired();

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<NurseProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.LicenseCountry)
            .WithMany()
            .HasForeignKey(p => p.LicenseCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CurrentCountry)
            .WithMany()
            .HasForeignKey(p => p.CurrentCountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
