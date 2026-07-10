using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Employers;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class EmployerProfileConfiguration : IEntityTypeConfiguration<EmployerProfile>
{
    public void Configure(EntityTypeBuilder<EmployerProfile> builder)
    {
        builder.ToTable("EmployerProfiles");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.Property(p => p.JobTitle).HasMaxLength(160);
        builder.Property(p => p.Department).HasMaxLength(160);

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<EmployerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
