using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Domain.Tests.Payments;

public class PaymentEntityTests
{
    [Fact]
    public void PaymentProduct_DefaultType_IsExamAccess()
    {
        var product = new PaymentProduct();

        Assert.Equal(PaymentProductType.ExamAccess, product.Type);
    }

    [Fact]
    public void PaymentProduct_DefaultActiveState_IsTrue()
    {
        var product = new PaymentProduct();

        Assert.True(product.IsActive);
    }

    [Fact]
    public void PaymentProduct_CreateExamAccess_NormalizesCurrencyAndUsesMinorUnitAmount()
    {
        var product = PaymentProduct.CreateExamAccess(Guid.NewGuid(), "  NCLEX  ", null, "usd", 4999);

        Assert.Equal(PaymentProductType.ExamAccess, product.Type);
        Assert.Equal("NCLEX", product.Name);
        Assert.Equal("USD", product.Currency);
        Assert.Equal(4999, product.UnitAmountMinor);
    }

    [Fact]
    public void PaymentOrder_CreatePending_SetsPendingPaymentStatusAndTotal()
    {
        var item = CreateItem(2499);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), item, new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PaymentOrderStatus.PendingPayment, order.Status);
        Assert.Equal(2499, order.TotalAmountMinor);
        Assert.Equal("USD", order.Currency);
        Assert.Null(order.PaidAt);
        Assert.Null(order.CancelledAt);
    }

    [Fact]
    public void PaymentOrder_CreatePending_SetsExpiresAtToCreatedAtPlusThirtyMinutes()
    {
        var createdAt = new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), createdAt);

        Assert.Equal(createdAt.AddMinutes(30), order.ExpiresAt);
    }

    [Fact]
    public void PaymentOrder_CancelPending_SetsCancelledStatusAndTimestamp()
    {
        var timestamp = new DateTime(2026, 7, 12, 11, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), timestamp.AddMinutes(-10));

        order.Cancel(timestamp);

        Assert.Equal(PaymentOrderStatus.Cancelled, order.Status);
        Assert.Equal(timestamp, order.CancelledAt);
    }

    [Fact]
    public void PaymentOrder_CancelTerminalStatus_ThrowsInvalidOperationException()
    {
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), DateTime.UtcNow);
        order.Status = PaymentOrderStatus.Expired;

        Assert.Throws<InvalidOperationException>(() => order.Cancel(DateTime.UtcNow));
    }

    [Fact]
    public void PaymentOrder_ExpirePastDuePending_SetsExpiredStatus()
    {
        var createdAt = new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc);
        var order = PaymentOrder.CreatePending(Guid.NewGuid(), CreateItem(1000), createdAt);

        var changed = order.ExpireIfPastDue(createdAt.AddMinutes(31));

        Assert.True(changed);
        Assert.Equal(PaymentOrderStatus.Expired, order.Status);
    }

    [Fact]
    public void PaymentOrderItem_CreateSnapshot_CopiesProductFieldsAndLineTotal()
    {
        var examId = Guid.NewGuid();
        var product = PaymentProduct.CreateExamAccess(examId, "Exam Access", "Description", "usd", 3500);

        var item = PaymentOrderItem.CreateSnapshot(product);

        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal("Exam Access", item.ProductNameSnapshot);
        Assert.Equal(PaymentProductType.ExamAccess, item.ProductTypeSnapshot);
        Assert.Equal(examId, item.ExamIdSnapshot);
        Assert.Equal("USD", item.Currency);
        Assert.Equal(3500, item.UnitAmountMinor);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(3500, item.LineTotalAmountMinor);
    }

    private static PaymentOrderItem CreateItem(long amount)
    {
        return new PaymentOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductNameSnapshot = "Exam Access",
            ProductTypeSnapshot = PaymentProductType.ExamAccess,
            ExamIdSnapshot = Guid.NewGuid(),
            Currency = "USD",
            UnitAmountMinor = amount,
            Quantity = 1,
            LineTotalAmountMinor = amount
        };
    }
}
