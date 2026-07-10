using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class NurseCertificateConfiguration : IEntityTypeConfiguration<NurseCertificate>
{
    public void Configure(EntityTypeBuilder<NurseCertificate> builder)
    {
        builder.ToTable("NurseCertificates");

        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.NurseProfileId);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.IssuingOrganization).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CredentialId).HasMaxLength(160);
        builder.Property(c => c.CredentialUrl).HasMaxLength(500);

        builder.HasOne(c => c.NurseProfile)
            .WithMany()
            .HasForeignKey(c => c.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
