using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Common;
using NursingPlatform.Application.Exams.DTOs;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.Application.Exams.Queries.ListExams;

public class ListExamsQueryHandler : IRequestHandler<ListExamsQuery, PaginatedResult<ExamCatalogItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListExamsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<PaginatedResult<ExamCatalogItemDto>> Handle(ListExamsQuery request, CancellationToken cancellationToken)
    {
        var nurseProfileId = await ExamHandlerHelpers.GetCurrentNurseProfileIdAsync(_context, _nurseRoleGuard, cancellationToken);
        var now = DateTime.UtcNow;

        var query = _context.Exams
            .Where(e => e.Status == ExamStatus.Published);

        if (request.CountryId is not null)
        {
            query = query.Where(e => e.CountryId == request.CountryId);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(e => e.ExamCategoryId == request.CategoryId);
        }

        var rows = await query
            .Join(_context.Countries, e => e.CountryId, c => c.Id, (exam, country) => new { exam, country })
            .GroupJoin(_context.ExamCategories, ec => ec.exam.ExamCategoryId, category => category.Id,
                (ec, categories) => new { ec.exam, ec.country, category = categories.FirstOrDefault() })
            .ToListAsync(cancellationToken);

        var examIds = rows.Select(r => r.exam.Id).ToList();
        var versions = await _context.ExamVersions
            .Where(v => examIds.Contains(v.ExamId) && v.Status == ExamVersionStatus.Published)
            .GroupBy(v => v.ExamId)
            .Select(g => g.OrderByDescending(v => v.VersionNumber).First())
            .ToListAsync(cancellationToken);
        var versionByExam = versions.ToDictionary(v => v.ExamId);

        var grants = await _context.ExamAccessGrants
            .Where(g => g.NurseProfileId == nurseProfileId
                && examIds.Contains(g.ExamId)
                && (g.ExpiresAt == null || g.ExpiresAt > now))
            .Select(g => g.ExamId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var grantedExamIds = grants.ToHashSet();

        var paidProductExamIds = await _context.PaymentProducts
            .Where(p => examIds.Contains(p.ExamId)
                && p.Type == PaymentProductType.ExamAccess
                && p.IsActive
                && p.UnitAmountMinor > 0)
            .Select(p => p.ExamId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var paidProductExamIdSet = paidProductExamIds.ToHashSet();

        var items = rows
            .Where(r => versionByExam.ContainsKey(r.exam.Id))
            .Select(r =>
            {
                var version = versionByExam[r.exam.Id];
                var isFree = r.exam.IsFree && !paidProductExamIdSet.Contains(r.exam.Id);
                var canStart = isFree || grantedExamIds.Contains(r.exam.Id);
                return ExamMapping.ToCatalogItem(r.exam, version, r.country.Name, r.category?.Name, isFree, canStart);
            })
            .Where(i => i.CanStart)
            .OrderBy(i => i.CountryName)
            .ThenBy(i => i.CategoryName)
            .ThenBy(i => i.Title)
            .ThenBy(i => i.Id)
            .ToList();

        return new PaginatedResult<ExamCatalogItemDto>
        {
            Items = items.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = items.Count
        };
    }
}
