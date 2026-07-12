using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.Queries.ListMyPaymentOrders;
using NursingPlatform.Application.Payments.Queries.ListPaymentProducts;

namespace NursingPlatform.Application.Tests.Payments;

public class PaymentValidatorTests
{
    [Fact]
    public void Validate_Product_WithInvalidCurrency_ShouldHaveError()
    {
        var validator = new CreateAdminPaymentProductCommandValidator();
        var request = ValidCreateRequest();
        request.Currency = "US";

        var result = validator.Validate(new CreateAdminPaymentProductCommand { Request = request });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains(nameof(CreateAdminPaymentProductRequest.Currency)));
    }

    [Fact]
    public void Validate_Product_WithNonPositiveAmount_ShouldHaveError()
    {
        var validator = new CreateAdminPaymentProductCommandValidator();
        var request = ValidCreateRequest();
        request.UnitAmountMinor = 0;

        var result = validator.Validate(new CreateAdminPaymentProductCommand { Request = request });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains(nameof(CreateAdminPaymentProductRequest.UnitAmountMinor)));
    }

    [Fact]
    public void Validate_CreateOrder_WithEmptyProductId_ShouldHaveError()
    {
        var validator = new CreateMyPaymentOrderCommandValidator();

        var result = validator.Validate(new CreateMyPaymentOrderCommand
        {
            Request = new CreatePaymentOrderRequest { ProductId = Guid.Empty }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains(nameof(CreatePaymentOrderRequest.ProductId)));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ListProducts_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var validator = new ListPaymentProductsQueryValidator();

        var result = validator.Validate(new ListPaymentProductsQuery { Page = page, PageSize = pageSize });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ListOrders_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var validator = new ListMyPaymentOrdersQueryValidator();

        var result = validator.Validate(new ListMyPaymentOrdersQuery { Page = page, PageSize = pageSize });

        Assert.False(result.IsValid);
    }

    private static CreateAdminPaymentProductRequest ValidCreateRequest()
    {
        return new CreateAdminPaymentProductRequest
        {
            ExamId = Guid.NewGuid(),
            Name = "Exam Access",
            Currency = "USD",
            UnitAmountMinor = 1000
        };
    }
}
