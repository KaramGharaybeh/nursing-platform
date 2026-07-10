using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseCvDocumentConfiguration : IEntityTypeConfiguration<NurseCvDocument>
{
    public void Configure(EntityTypeBuilder<NurseCvDocument> builder)
    {
        builder.ToTable("NurseCvDocuments");

        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.NurseProfileId).IsUnique();

        builder.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(d => d.StorageKey).IsRequired().HasMaxLength(500);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FileSizeBytes).IsRequired();
        builder.Property(d => d.UploadedAt).IsRequired();

        builder.HasOne(d => d.NurseProfile)
            .WithMany()
            .HasForeignKey(d => d.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
