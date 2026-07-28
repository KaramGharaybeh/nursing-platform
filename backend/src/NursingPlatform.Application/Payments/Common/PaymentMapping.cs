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

    public static PaymentCheckoutSessionDto ToCheckoutSessionDto(PaymentCheckoutSession session)
    {
        return new PaymentCheckoutSessionDto
        {
            Id = session.Id,
            PaymentOrderId = session.PaymentOrderId,
            Status = session.Status.ToString(),
            ProviderName = session.ProviderName,
            CheckoutUrl = session.CheckoutUrl,
            Currency = session.Currency,
            AmountMinor = session.AmountMinor,
            ExpiresAt = session.ExpiresAt,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
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
            LineTotalAmountMinor = item.LineTotalAmountMinor,
            SourceType = item.SourceType.ToString(),
            SourceId = item.SourceId,
            PackageSnapshot = item.PackageOrderItemSnapshot is null
                ? null
                : new PaymentPackageSnapshotDto
                {
                    PackageOfferId = item.PackageOrderItemSnapshot.PackageOfferId,
                    PackageOfferTitle = item.PackageOrderItemSnapshot.PackageOfferTitle,
                    PackageOfferSlug = item.PackageOrderItemSnapshot.PackageOfferSlug,
                    PackageOfferSummary = item.PackageOrderItemSnapshot.PackageOfferSummary,
                    PackageDefinitionId = item.PackageOrderItemSnapshot.PackageDefinitionId,
                    PackageDefinitionTitle = item.PackageOrderItemSnapshot.PackageDefinitionTitle,
                    PackageDefinitionSlug = item.PackageOrderItemSnapshot.PackageDefinitionSlug,
                    CountryId = item.PackageOrderItemSnapshot.CountryId,
                    ExamCategoryId = item.PackageOrderItemSnapshot.ExamCategoryId,
                    PackageVersionId = item.PackageOrderItemSnapshot.PackageVersionId,
                    PackageVersionNumber = item.PackageOrderItemSnapshot.PackageVersionNumber,
                    IncludedExamId = item.PackageOrderItemSnapshot.IncludedExamId,
                    IncludedExamVersionId = item.PackageOrderItemSnapshot.IncludedExamVersionId,
                    IncludedExamTitle = item.PackageOrderItemSnapshot.IncludedExamTitle,
                    ReportingProfilePublicationId = item.PackageOrderItemSnapshot.ReportingProfilePublicationId,
                    PracticeCollectionVersionId = item.PackageOrderItemSnapshot.PracticeCollectionVersionId,
                    StudyMaterialVersionIds = item.PackageOrderItemSnapshot.StudyMaterialVersionIds.ToList(),
                    PriceAmountMinor = item.PackageOrderItemSnapshot.PriceAmountMinor,
                    Currency = item.PackageOrderItemSnapshot.Currency,
                    AccessDurationDays = item.PackageOrderItemSnapshot.AccessDurationDays,
                    OrderCreatedAt = item.PackageOrderItemSnapshot.OrderCreatedAt
                }
        };
    }
}
