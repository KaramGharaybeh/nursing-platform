using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
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
    public void Validate_AdminUpdateProduct_RequestDoesNotContainTypeExamIdOrIsActive()
    {
        var properties = typeof(UpdateAdminPaymentProductRequest).GetProperties().Select(p => p.Name).ToArray();

        Assert.DoesNotContain("Type", properties);
        Assert.DoesNotContain("ExamId", properties);
        Assert.DoesNotContain("IsActive", properties);
        Assert.Equal(["Name", "Description", "Currency", "UnitAmountMinor"], properties);
    }
}
