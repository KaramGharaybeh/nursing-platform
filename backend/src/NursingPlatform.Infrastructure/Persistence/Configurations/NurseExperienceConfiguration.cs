using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseExperienceConfiguration : IEntityTypeConfiguration<NurseExperience>
{
    public void Configure(EntityTypeBuilder<NurseExperience> builder)
    {
        builder.ToTable("NurseExperiences");

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.NurseProfileId);
        builder.HasIndex(e => e.CountryId);

        builder.Property(e => e.FacilityName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(160);
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.IsCurrent).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);

        builder.HasOne(e => e.NurseProfile)
            .WithMany()
            .HasForeignKey(e => e.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
