using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Employers;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class EmployerOrganizationConfiguration : IEntityTypeConfiguration<EmployerOrganization>
{
    public void Configure(EntityTypeBuilder<EmployerOrganization> builder)
    {
        builder.ToTable("EmployerOrganizations");

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.EmployerProfileId).IsUnique();
        builder.HasIndex(o => o.CountryId);

        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Type).HasMaxLength(100);
        builder.Property(o => o.WebsiteUrl).HasMaxLength(500);
        builder.Property(o => o.City).HasMaxLength(120);
        builder.Property(o => o.AddressLine1).HasMaxLength(200);
        builder.Property(o => o.AddressLine2).HasMaxLength(200);
        builder.Property(o => o.PostalCode).HasMaxLength(40);
        builder.Property(o => o.Description).HasMaxLength(2000);

        builder.HasOne(o => o.EmployerProfile)
            .WithOne(p => p.Organization)
            .HasForeignKey<EmployerOrganization>(o => o.EmployerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Country)
            .WithMany()
            .HasForeignKey(o => o.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
