using System.Reflection;
using NursingPlatform.Application.Payments.Abstractions;
using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;
using NursingPlatform.Application.Payments.DTOs;

namespace NursingPlatform.Application.Tests.Payments;

public class PaymentDtoSecurityTests
{
    [Theory]
    [InlineData(typeof(PaymentProductDto))]
    [InlineData(typeof(PaymentOrderDto))]
    [InlineData(typeof(PaymentOrderItemDto))]
    public void PaymentDtos_ShouldNotExposeAccountInternalsProviderFieldsCardDataOrEntities(Type dtoType)
    {
        var forbidden = new[]
        {
            "UserId",
            "PasswordHash",
            "Roles",
            "Permissions",
            "Token",
            "Provider",
            "Card",
            "Webhook",
            "Secret",
            "PaymentProduct",
            "PaymentOrder",
            "PaymentOrderItem",
            "NurseProfile",
            "ExamAccessGrant"
        };

        var properties = dtoType.GetProperties().Select(p => p.Name).ToList();

        foreach (var field in forbidden)
        {
            Assert.DoesNotContain(properties, p => p.Contains(field, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Validate_CreateOrder_RequestContainsOnlyProductId()
    {
        var properties = typeof(CreatePaymentOrderRequest).GetProperties().Select(p => p.Name).ToArray();

        Assert.Equal(["ProductId"], properties);
    }

    [Fact]
    public void StartCheckoutRequest_ShouldContainOnlyOptionalIdempotencyKey()
    {
        var properties = typeof(StartPaymentCheckoutRequest).GetProperties().Select(p => p.Name).ToArray();
        var idempotencyKey = typeof(StartPaymentCheckoutRequest).GetProperty(nameof(StartPaymentCheckoutRequest.IdempotencyKey))!;
        var nullability = new NullabilityInfoContext().Create(idempotencyKey);

        Assert.Equal(["IdempotencyKey"], properties);
        Assert.Equal(NullabilityState.Nullable, nullability.WriteState);
    }

    [Fact]
    public void PaymentCheckoutDto_ShouldNotExposeSensitiveFieldsCardDataSecretsRawPayloadsOrEntities()
    {
        var forbidden = new[]
        {
            "NurseProfileId",
            "UserId",
            "PasswordHash",
            "Token",
            "Secret",
            "Card",
            "Webhook",
            "Payload",
            "Raw",
            "Idempotency",
            "Fingerprint",
            "PaymentIntent",
            "ProviderCallLease",
            "PaymentOrderItem",
            "NurseProfile",
            "ExamAccessGrant"
        };

        var properties = typeof(PaymentCheckoutSessionDto).GetProperties().Select(p => p.Name).ToList();

        foreach (var field in forbidden)
        {
            Assert.DoesNotContain(properties, p => p.Contains(field, StringComparison.OrdinalIgnoreCase));
        }

        Assert.Equal(
            ["Id", "PaymentOrderId", "Status", "ProviderName", "CheckoutUrl", "Currency", "AmountMinor", "ExpiresAt", "CreatedAt", "UpdatedAt"],
            properties);
    }

    [Fact]
    public void PaymentCheckoutProviderInterface_ShouldNotExposeCardDataSecretsRawPayloadsOrProviderNameInResult()
    {
        var method = typeof(IPaymentCheckoutProvider).GetMethod(nameof(IPaymentCheckoutProvider.CreateCheckoutSessionAsync));
        var providerProperties = typeof(IPaymentCheckoutProvider).GetProperties().Select(p => p.Name).ToArray();
        var resultProperties = typeof(CreatePaymentCheckoutProviderSessionResult).GetProperties().Select(p => p.Name).ToArray();

        Assert.NotNull(method);
        Assert.Equal(["ProviderName"], providerProperties);
        Assert.DoesNotContain("ProviderName", resultProperties);
        Assert.DoesNotContain(resultProperties, p => p.Contains("Card", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, p => p.Contains("Secret", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, p => p.Contains("Payload", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, p => p.Contains("Raw", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PaymentCheckoutProviderResult_ShouldContainProviderCheckoutIdOptionalPaymentIntentCheckoutUrlAndExpiryOnly()
    {
        var properties = typeof(CreatePaymentCheckoutProviderSessionResult).GetProperties().Select(p => p.Name).ToArray();

        Assert.Equal(["ProviderCheckoutSessionId", "ProviderPaymentIntentId", "CheckoutUrl", "ExpiresAt"], properties);
    }

    [Fact]
    public void PaymentCheckoutProviderRequest_ShouldNotExposeCardSecretsRawPayloadsOrInternalLeaseFields()
    {
        var properties = typeof(CreatePaymentCheckoutProviderSessionRequest).GetProperties().Select(p => p.Name).ToArray();

        Assert.Equal(
            ["PaymentOrderId", "CheckoutSessionId", "ProviderClientReference", "Currency", "AmountMinor", "Description", "SuccessUrl", "CancelUrl", "ExpiresAt"],
            properties);
        Assert.DoesNotContain(properties, p => p.Contains("Card", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Secret", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Payload", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Raw", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, p => p.Contains("Lease", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_AdminUpdateProduct_RequestDoesNotContainTypeExamIdOrIsActive()
    {
        var properties = typeof(UpdateAdminPaymentProductRequest).GetProperties().Select(p => p.Name).ToArray();

        Assert.DoesNotContain("Type", properties);
        Assert.DoesNotContain("ExamId", properties);
        Assert.DoesNotContain("IsActive", properties);
        Assert.Equal(["Name", "Description", "Currency", "UnitAmountMinor"], properties);
    }
}
