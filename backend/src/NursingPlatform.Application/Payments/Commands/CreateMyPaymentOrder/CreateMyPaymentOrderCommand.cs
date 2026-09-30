using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Payments.Common;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;

public class CreateMyPaymentOrderCommand : IRequest<PaymentOrderDto>
{
    public CreatePaymentOrderRequest Request { get; set; } = new();
}

public class CreateMyPaymentOrderCommandValidator : AbstractValidator<CreateMyPaymentOrderCommand>
{
    public CreateMyPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request)
            .Must(request => HasExactlyOnePurchaseSource(request.ProductId, request.PackageOfferId))
            .WithMessage("Exactly one purchase source is required.");
        RuleFor(x => x.Request.ProductId)
            .NotEqual(Guid.Empty)
            .When(x => x.Request?.ProductId.HasValue == true);
        RuleFor(x => x.Request.PackageOfferId)
            .NotEqual(Guid.Empty)
            .When(x => x.Request?.PackageOfferId.HasValue == true);
    }

    private static bool HasExactlyOnePurchaseSource(Guid? productId, Guid? packageOfferId)
    {
        return productId.HasValue != packageOfferId.HasValue;
    }
}

public class CreateMyPaymentOrderCommandHandler : IRequestHandler<CreateMyPaymentOrderCommand, PaymentOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CreateMyPaymentOrderCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaymentOrderDto> Handle(CreateMyPaymentOrderCommand request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);

        if (request.Request.PackageOfferId.HasValue)
        {
            return await CreatePackageOrderAsync(nurseProfileId, request.Request.PackageOfferId.Value, cancellationToken);
        }

        var row = await _context.PaymentProducts
            .Where(p => p.Id == request.Request.ProductId!.Value)
            .Join(_context.Exams,
                product => product.ExamId,
                exam => exam.Id,
                (product, exam) => new { product, exam })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Payment product was not found.");
        }

        if (!row.product.IsActive)
        {
            throw new InvalidOperationException("Payment product is not active.");
        }

        if (row.exam.Status != ExamStatus.Published)
        {
            throw new InvalidOperationException("Payment product is not currently purchasable.");
        }

        var item = PaymentOrderItem.CreateSnapshot(row.product);
        var order = PaymentOrder.CreatePending(nurseProfileId, item, DateTime.UtcNow);

        _context.PaymentOrders.Add(order);
        _context.PaymentOrderItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return PaymentMapping.ToOrderDto(order, [item]);
    }

    private async Task<PaymentOrderDto> CreatePackageOrderAsync(
        Guid nurseProfileId,
        Guid packageOfferId,
        CancellationToken cancellationToken)
    {
        var orderCreatedAt = DateTime.UtcNow;
        var packageFacts = await LoadSellablePackageFactsAsync(packageOfferId, cancellationToken);

        var snapshot = PackageOrderItemSnapshot.Create(
            packageFacts.Offer.Id,
            packageFacts.Offer.Title,
            packageFacts.Offer.Slug,
            packageFacts.Offer.Summary,
            packageFacts.Definition.Id,
            packageFacts.Definition.Title,
            packageFacts.Definition.Slug,
            packageFacts.Definition.CountryId,
            packageFacts.Definition.ExamCategoryId,
            packageFacts.PackageVersion.Id,
            packageVersionNumber: 1,
            packageFacts.Exam.Id,
            packageFacts.ExamVersion.Id,
            packageFacts.Exam.Title,
            packageFacts.ReportingProfile.Id,
            packageFacts.PracticeCollectionVersion.Id,
            packageFacts.StudyMaterialVersionIds,
            packageFacts.Offer.PriceAmountMinor,
            packageFacts.Offer.Currency,
            packageFacts.Offer.AccessDurationDays,
            orderCreatedAt);
        var item = PaymentOrderItem.CreatePackageOfferSnapshot(snapshot);
        var order = PaymentOrder.CreatePending(nurseProfileId, item, orderCreatedAt);

        _context.PaymentOrders.Add(order);
        _context.PaymentOrderItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return PaymentMapping.ToOrderDto(order, [item]);
    }

    private async Task<PackageOrderFacts> LoadSellablePackageFactsAsync(Guid packageOfferId, CancellationToken cancellationToken)
    {
        var offer = await _context.PreparationPackageOffers.FirstOrDefaultAsync(o => o.Id == packageOfferId, cancellationToken)
            ?? throw new KeyNotFoundException("Preparation package offer was not found.");
        if (offer.Status != PreparationPackageOfferStatus.Active)
        {
            throw new InvalidOperationException("Preparation package offer is not currently purchasable.");
        }

        var definition = await _context.PreparationPackageDefinitions.FirstOrDefaultAsync(d => d.Id == offer.PreparationPackageDefinitionId, cancellationToken)
            ?? throw new InvalidOperationException("Preparation package definition was not found.");
        var packageVersion = await _context.PreparationPackageVersions.FirstOrDefaultAsync(v => v.Id == offer.PreparationPackageVersionId, cancellationToken)
            ?? throw new InvalidOperationException("Preparation package version was not found.");
        if (packageVersion.Status != PreparationPackageVersionStatus.Published || !packageVersion.ContentIsolationConfirmed)
        {
            throw new InvalidOperationException("Preparation package version is not currently purchasable.");
        }

        var examVersion = await _context.ExamVersions.FirstOrDefaultAsync(v => v.Id == packageVersion.ExamVersionId, cancellationToken)
            ?? throw new InvalidOperationException("Package exam version was not found.");
        if (examVersion.Status != ExamVersionStatus.Published)
        {
            throw new InvalidOperationException("Package exam version is not currently purchasable.");
        }

        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examVersion.ExamId, cancellationToken)
            ?? throw new InvalidOperationException("Package exam was not found.");
        if (exam.Status != ExamStatus.Published || exam.CountryId != definition.CountryId || exam.ExamCategoryId != definition.ExamCategoryId)
        {
            throw new InvalidOperationException("Package exam does not match the package context.");
        }

        var reportingProfile = await _context.ReportingProfilePublications.FirstOrDefaultAsync(p => p.Id == packageVersion.ReportingProfilePublicationId, cancellationToken)
            ?? throw new InvalidOperationException("Package reporting profile was not found.");
        if (reportingProfile.Status != PublicationStatus.Published || reportingProfile.ExamVersionId != examVersion.Id)
        {
            throw new InvalidOperationException("Package reporting profile is not currently purchasable.");
        }

        var practiceCollectionVersion = await _context.PracticeCollectionVersions.FirstOrDefaultAsync(v => v.Id == packageVersion.PracticeCollectionVersionId, cancellationToken)
            ?? throw new InvalidOperationException("Package practice collection version was not found.");
        if (practiceCollectionVersion.Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException("Package practice collection version is not currently purchasable.");
        }

        var materialVersionIds = await _context.PreparationPackageVersionMaterials
            .Where(m => m.PreparationPackageVersionId == packageVersion.Id)
            .OrderBy(m => m.SortOrder)
            .Select(m => m.StudyMaterialVersionId)
            .ToListAsync(cancellationToken);
        if (materialVersionIds.Count == 0)
        {
            throw new InvalidOperationException("Preparation package version must include materials.");
        }

        var publishedMaterialCount = await _context.StudyMaterialVersions
            .CountAsync(v => materialVersionIds.Contains(v.Id) && v.Status == PublicationStatus.Published, cancellationToken);
        if (publishedMaterialCount != materialVersionIds.Count)
        {
            throw new InvalidOperationException("Preparation package materials are not currently purchasable.");
        }

        return new PackageOrderFacts(
            offer,
            definition,
            packageVersion,
            exam,
            examVersion,
            reportingProfile,
            practiceCollectionVersion,
            materialVersionIds);
    }

    private sealed record PackageOrderFacts(
        PreparationPackageOffer Offer,
        PreparationPackageDefinition Definition,
        PreparationPackageVersion PackageVersion,
        Exam Exam,
        ExamVersion ExamVersion,
        ReportingProfilePublication ReportingProfile,
        PracticeCollectionVersion PracticeCollectionVersion,
        IReadOnlyCollection<Guid> StudyMaterialVersionIds);
}
