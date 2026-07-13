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

    [Fact]
    public void PaymentCheckoutSessionConfiguration_UsesExpectedTableNameAndPrimaryKey()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession));

        Assert.NotNull(entity);
        Assert.Equal("PaymentCheckoutSessions", entity.GetTableName());
        Assert.Equal("Id", Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_StoresStatusAsStringWithMaxLength()
    {
        var property = CheckoutSessionProperty(nameof(PaymentCheckoutSession.Status));

        Assert.Equal(32, property.GetMaxLength());
        Assert.NotNull(property.GetTypeMapping().Converter);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresRequiredProviderNameCurrencyAndAmount()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!;

        Assert.False(entity.FindProperty(nameof(PaymentCheckoutSession.ProviderName))!.IsNullable);
        Assert.Equal(100, entity.FindProperty(nameof(PaymentCheckoutSession.ProviderName))!.GetMaxLength());
        Assert.False(entity.FindProperty(nameof(PaymentCheckoutSession.Currency))!.IsNullable);
        Assert.Equal(3, entity.FindProperty(nameof(PaymentCheckoutSession.Currency))!.GetMaxLength());
        Assert.False(entity.FindProperty(nameof(PaymentCheckoutSession.AmountMinor))!.IsNullable);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_DoesNotGloballyRequireProviderCheckoutSessionIdOrCheckoutUrl()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!;

        Assert.True(entity.FindProperty(nameof(PaymentCheckoutSession.ProviderCheckoutSessionId))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(PaymentCheckoutSession.CheckoutUrl))!.IsNullable);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresOrderAndNurseRestrictDeleteRelationships()
    {
        var foreignKeys = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!.GetForeignKeys().ToList();

        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType == typeof(PaymentOrder)
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PaymentCheckoutSession.PaymentOrderId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(foreignKeys, fk => fk.PrincipalEntityType.ClrType.Name == "NurseProfile"
            && fk.Properties.Select(p => p.Name).SequenceEqual([nameof(PaymentCheckoutSession.NurseProfileId)])
            && fk.DeleteBehavior == DeleteBehavior.Restrict);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresUniqueActiveCheckoutSessionPerOrder()
    {
        var index = FindCheckoutSessionIndex([nameof(PaymentCheckoutSession.PaymentOrderId)], unique: true);

        Assert.Contains("Created", index.GetFilter(), StringComparison.Ordinal);
        Assert.Contains("ProviderPending", index.GetFilter(), StringComparison.Ordinal);
        Assert.DoesNotContain("CreationRejected", index.GetFilter(), StringComparison.Ordinal);
        Assert.DoesNotContain("Expired", index.GetFilter(), StringComparison.Ordinal);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresUniqueProviderClientReference()
    {
        FindCheckoutSessionIndex([nameof(PaymentCheckoutSession.ProviderClientReference)], unique: true);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresUniqueProviderNameAndCheckoutSessionIdWhenNotNull()
    {
        var index = FindCheckoutSessionIndex(
            [nameof(PaymentCheckoutSession.ProviderName), nameof(PaymentCheckoutSession.ProviderCheckoutSessionId)],
            unique: true);

        Assert.Contains(nameof(PaymentCheckoutSession.ProviderCheckoutSessionId), index.GetFilter(), StringComparison.Ordinal);
        Assert.Contains("IS NOT NULL", index.GetFilter(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresUniqueNurseAndIdempotencyKeyHashWhenNotNull()
    {
        var index = FindCheckoutSessionIndex(
            [nameof(PaymentCheckoutSession.NurseProfileId), nameof(PaymentCheckoutSession.IdempotencyKeyHash)],
            unique: true);

        Assert.Contains(nameof(PaymentCheckoutSession.IdempotencyKeyHash), index.GetFilter(), StringComparison.Ordinal);
        Assert.Contains("IS NOT NULL", index.GetFilter(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresReuseOwnershipAndPaymentIntentIndexes()
    {
        FindCheckoutSessionIndex([
            nameof(PaymentCheckoutSession.PaymentOrderId),
            nameof(PaymentCheckoutSession.Status),
            nameof(PaymentCheckoutSession.ExpiresAt)]);
        FindCheckoutSessionIndex([
            nameof(PaymentCheckoutSession.NurseProfileId),
            nameof(PaymentCheckoutSession.PaymentOrderId)]);
        FindCheckoutSessionIndex([
            nameof(PaymentCheckoutSession.ProviderName),
            nameof(PaymentCheckoutSession.ProviderPaymentIntentId)]);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresNullableProviderCallLeaseFields()
    {
        var entity = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!;

        Assert.True(entity.FindProperty(nameof(PaymentCheckoutSession.ProviderCallLeaseId))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(PaymentCheckoutSession.ProviderCallLeaseExpiresAt))!.IsNullable);
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_ConfiguresRequestFingerprintHash()
    {
        var property = CheckoutSessionProperty(nameof(PaymentCheckoutSession.RequestFingerprintHash));

        Assert.False(property.IsNullable);
        Assert.Equal(128, property.GetMaxLength());
    }

    [Fact]
    public void PaymentCheckoutSessionConfiguration_DoesNotAddCardSecretRawPayloadWebhookProviderCancellationOrGrantColumns()
    {
        var forbidden = new[] { "Card", "Secret", "Raw", "Payload", "Webhook", "Cancellation", "Grant", "ClientSecret" };
        var properties = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        foreach (var field in forbidden)
        {
            Assert.DoesNotContain(properties, p => p.Contains(field, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IProperty CheckoutSessionProperty(string propertyName)
    {
        return CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!.FindProperty(propertyName)!;
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IIndex FindCheckoutSessionIndex(string[] propertyNames, bool? unique = null)
    {
        var indexes = CreateDbContext().Model.FindEntityType(typeof(PaymentCheckoutSession))!.GetIndexes().ToList();
        return indexes.Single(i => i.Properties.Select(p => p.Name).SequenceEqual(propertyNames)
            && (unique is null || i.IsUnique == unique));
    }
}
