using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public class PaymentConfigurationTests
{
    [Theory]
    [InlineData(typeof(PaymentProduct), "PaymentProducts")]
    [InlineData(typeof(PaymentOrder), "PaymentOrders")]
    [InlineData(typeof(PaymentOrderItem), "PaymentOrderItems")]
    public void PaymentConfiguration_UsesExpectedTableNamesAndPrimaryKeys(Type entityType, string tableName)
    {
        var entity = CreateDbContext().Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());
        Assert.Equal("Id", Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
    }

    [Theory]
    [InlineData(typeof(PaymentProduct), nameof(PaymentProduct.Type))]
    [InlineData(typeof(PaymentOrder), nameof(PaymentOrder.Status))]
    [InlineData(typeof(PaymentOrderItem), nameof(PaymentOrderItem.ProductTypeSnapshot))]
    public void PaymentConfiguration_StoresEnumsAsStringsWithMaxLength(Type entityType, string propertyName)
    {
        var property = CreateDbContext().Model.FindEntityType(entityType)!.FindProperty(propertyName)!;

        Assert.Equal(32, property.GetMaxLength());
        Assert.NotNull(property.GetTypeMapping().Converter);
    }

    [Fact]
    public void PaymentConfiguration_StoresMoneyAsLongMinorUnits()
    {
        var context = CreateDbContext();

        Assert.Equal(typeof(long), context.Model.FindEntityType(typeof(PaymentProduct))!
            .FindProperty(nameof(PaymentProduct.UnitAmountMinor))!.ClrType);
        Assert.Equal(typeof(long), context.Model.FindEntityType(typeof(PaymentOrder))!
            .FindProperty(nameof(PaymentOrder.TotalAmountMinor))!.ClrType);
        Assert.Equal(typeof(long), context.Model.FindEntityType(typeof(PaymentOrderItem))!
            .FindProperty(nameof(PaymentOrderItem.LineTotalAmountMinor))!.ClrType);
    }

    [Fact]
    public void PaymentConfiguration_ConfiguresProductCatalogIndexes()
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(PaymentProduct))!.GetIndexes().ToList();

        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(PaymentProduct.Type), nameof(PaymentProduct.IsActive), nameof(PaymentProduct.ExamId), nameof(PaymentProduct.Name), nameof(PaymentProduct.Id)]));
        Assert.Contains(indexes, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(PaymentProduct.Type), nameof(PaymentProduct.ExamId)]));
    }

    [Fact]
    public void PaymentConfiguration_ConfiguresOrderOwnershipIndexes()
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(PaymentOrder))!.GetIndexes().ToList();

        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(PaymentOrder.NurseProfileId), nameof(PaymentOrder.CreatedAt), nameof(PaymentOrder.Id)]));
        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(PaymentOrder.NurseProfileId), nameof(PaymentOrder.Status), nameof(PaymentOrder.CreatedAt), nameof(PaymentOrder.Id)]));
    }

    [Fact]
    public void PaymentConfiguration_ConfiguresOrderItemIndex()
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(PaymentOrderItem))!.GetIndexes().ToList();

        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(PaymentOrderItem.OrderId), nameof(PaymentOrderItem.Id)]));
    }

    [Fact]
    public void PaymentConfiguration_ConfiguresRestrictDeleteRelationships()
    {
        var entityTypes = new[] { typeof(PaymentProduct), typeof(PaymentOrder), typeof(PaymentOrderItem) };
        var foreignKeys = entityTypes
            .SelectMany(t => CreateDbContext().Model.FindEntityType(t)!.GetForeignKeys())
            .ToList();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void PaymentConfiguration_DoesNotAddProviderColumns()
    {
        var properties = new[]
        {
            typeof(PaymentProduct),
            typeof(PaymentOrder),
            typeof(PaymentOrderItem)
        }.SelectMany(t => CreateDbContext().Model.FindEntityType(t)!.GetProperties().Select(p => p.Name));

        Assert.DoesNotContain(properties, p => p.Contains("Provider", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Checkout", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Card", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Webhook", StringComparison.OrdinalIgnoreCase));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
