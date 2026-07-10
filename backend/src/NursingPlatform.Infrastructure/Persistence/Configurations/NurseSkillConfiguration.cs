using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseSkillConfiguration : IEntityTypeConfiguration<NurseSkill>
{
    public void Configure(EntityTypeBuilder<NurseSkill> builder)
    {
        builder.ToTable("NurseSkills");

        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.NurseProfileId);
        builder.HasIndex(s => new { s.NurseProfileId, s.NormalizedName }).IsUnique();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.NormalizedName).IsRequired().HasMaxLength(100);

        builder.HasOne(s => s.NurseProfile)
            .WithMany()
            .HasForeignKey(s => s.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
