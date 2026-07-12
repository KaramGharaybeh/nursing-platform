using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Payments.Common;

internal static class PaymentMapping
{
    public static PaymentProductDto ToProductDto(PaymentProduct product, string examTitle)
    {
        return new PaymentProductDto
        {
            Id = product.Id,
            Type = product.Type.ToString(),
            ExamId = product.ExamId,
            ExamTitle = examTitle,
            Name = product.Name,
            Description = product.Description,
            Currency = product.Currency,
            UnitAmountMinor = product.UnitAmountMinor,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public static PaymentOrderDto ToOrderDto(PaymentOrder order, IReadOnlyCollection<PaymentOrderItem> items)
    {
        return new PaymentOrderDto
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            Currency = order.Currency,
            TotalAmountMinor = order.TotalAmountMinor,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ExpiresAt = order.ExpiresAt,
            PaidAt = order.PaidAt,
            CancelledAt = order.CancelledAt,
            Items = items
                .OrderBy(i => i.Id)
                .Select(ToOrderItemDto)
                .ToList()
        };
    }

    private static PaymentOrderItemDto ToOrderItemDto(PaymentOrderItem item)
    {
        return new PaymentOrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductNameSnapshot,
            ProductType = item.ProductTypeSnapshot.ToString(),
            ExamId = item.ExamIdSnapshot,
            Currency = item.Currency,
            UnitAmountMinor = item.UnitAmountMinor,
            Quantity = item.Quantity,
            LineTotalAmountMinor = item.LineTotalAmountMinor
        };
    }
}
