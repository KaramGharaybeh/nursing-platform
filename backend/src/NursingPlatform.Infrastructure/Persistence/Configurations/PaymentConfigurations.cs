using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class PaymentProductConfiguration : IEntityTypeConfiguration<PaymentProduct>
{
    public void Configure(EntityTypeBuilder<PaymentProduct> builder)
    {
        builder.ToTable("PaymentProducts");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.Type, p.ExamId }).IsUnique();
        builder.HasIndex(p => new { p.Type, p.IsActive, p.ExamId, p.Name, p.Id });
        builder.Property(p => p.Type).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.HasOne(p => p.Exam)
            .WithMany()
            .HasForeignKey(p => p.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("PaymentOrders");
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.NurseProfileId, o.CreatedAt, o.Id });
        builder.HasIndex(o => new { o.NurseProfileId, o.Status, o.CreatedAt, o.Id });
        builder.Property(o => o.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.HasOne(o => o.NurseProfile)
            .WithMany()
            .HasForeignKey(o => o.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentOrderItemConfiguration : IEntityTypeConfiguration<PaymentOrderItem>
{
    public void Configure(EntityTypeBuilder<PaymentOrderItem> builder)
    {
        builder.ToTable("PaymentOrderItems");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => new { i.OrderId, i.Id });
        builder.Property(i => i.ProductNameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductTypeSnapshot).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
