using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseEducationConfiguration : IEntityTypeConfiguration<NurseEducation>
{
    public void Configure(EntityTypeBuilder<NurseEducation> builder)
    {
        builder.ToTable("NurseEducation");

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.NurseProfileId);
        builder.HasIndex(e => e.CountryId);

        builder.Property(e => e.InstitutionName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Degree).IsRequired().HasMaxLength(160);
        builder.Property(e => e.FieldOfStudy).HasMaxLength(160);
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
