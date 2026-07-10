using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseLanguageConfiguration : IEntityTypeConfiguration<NurseLanguage>
{
    public void Configure(EntityTypeBuilder<NurseLanguage> builder)
    {
        builder.ToTable("NurseLanguages");

        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.NurseProfileId);
        builder.HasIndex(l => new { l.NurseProfileId, l.LanguageId }).IsUnique();

        builder.Property(l => l.Proficiency).IsRequired().HasMaxLength(50);

        builder.HasOne(l => l.NurseProfile)
            .WithMany()
            .HasForeignKey(l => l.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Language)
            .WithMany()
            .HasForeignKey(l => l.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
