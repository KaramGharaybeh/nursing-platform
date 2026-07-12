using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.Admin.Common;
using NursingPlatform.Application.Exams.Admin.DTOs;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Application.Exams.Admin.Exams;

public class CreateAdminExamRequest
{
    public Guid CountryId { get; set; }
    public Guid? ExamCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PassingScorePercentage { get; set; }
    public bool IsFree { get; set; } = true;
}

public class UpdateAdminExamRequest : CreateAdminExamRequest
{
}

public class ListAdminExamsQuery : IRequest<PaginatedResult<AdminExamDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CountryId { get; set; }
    public Guid? CategoryId { get; set; }
    public ExamStatus? Status { get; set; }
    public bool? IsFree { get; set; }
}

public class GetAdminExamQuery : IRequest<AdminExamDto>
{
    public Guid Id { get; set; }
}

public class CreateAdminExamCommand : IRequest<AdminExamDto>
{
    public CreateAdminExamRequest Request { get; set; } = new();
}

public class UpdateAdminExamCommand : IRequest<AdminExamDto>
{
    public Guid Id { get; set; }
    public UpdateAdminExamRequest Request { get; set; } = new();
}

public class ArchiveAdminExamCommand : IRequest<AdminExamDto>
{
    public Guid Id { get; set; }
}

public class DeleteAdminExamCommand : IRequest
{
    public Guid Id { get; set; }
}

public class ListAdminExamsQueryValidator : AbstractValidator<ListAdminExamsQuery>
{
    public ListAdminExamsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}

public class GetAdminExamQueryValidator : AbstractValidator<GetAdminExamQuery>
{
    public GetAdminExamQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CreateAdminExamCommandValidator : AbstractValidator<CreateAdminExamCommand>
{
    public CreateAdminExamCommandValidator()
    {
        RuleFor(x => x.Request.CountryId).NotEmpty();
        RuleFor(x => x.Request.ExamCategoryId).NotEqual(Guid.Empty).When(x => x.Request.ExamCategoryId.HasValue);
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
        RuleFor(x => x.Request.Instructions).MaximumLength(4000);
        RuleFor(x => x.Request.DurationMinutes).InclusiveBetween(1, AdminExamContentValidator.MaxDurationMinutes);
        RuleFor(x => x.Request.PassingScorePercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateAdminExamCommandValidator : AbstractValidator<UpdateAdminExamCommand>
{
    public UpdateAdminExamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CountryId).NotEmpty();
        RuleFor(x => x.Request.ExamCategoryId).NotEqual(Guid.Empty).When(x => x.Request.ExamCategoryId.HasValue);
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
        RuleFor(x => x.Request.Instructions).MaximumLength(4000);
        RuleFor(x => x.Request.DurationMinutes).InclusiveBetween(1, AdminExamContentValidator.MaxDurationMinutes);
        RuleFor(x => x.Request.PassingScorePercentage).InclusiveBetween(0, 100);
    }
}

public class ArchiveAdminExamCommandValidator : AbstractValidator<ArchiveAdminExamCommand>
{
    public ArchiveAdminExamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteAdminExamCommandValidator : AbstractValidator<DeleteAdminExamCommand>
{
    public DeleteAdminExamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class ListAdminExamsQueryHandler : IRequestHandler<ListAdminExamsQuery, PaginatedResult<AdminExamDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminExamsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AdminExamDto>> Handle(ListAdminExamsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Exams.AsQueryable();
        if (request.CountryId is not null)
        {
            query = query.Where(e => e.CountryId == request.CountryId);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(e => e.ExamCategoryId == request.CategoryId);
        }

        if (request.Status is not null)
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (request.IsFree is not null)
        {
            query = query.Where(e => e.IsFree == request.IsFree);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Join(_context.Countries, e => e.CountryId, c => c.Id, (exam, country) => new { exam, country })
            .GroupJoin(_context.ExamCategories, ec => ec.exam.ExamCategoryId, category => category.Id,
                (ec, categories) => new { ec.exam, ec.country, category = categories.FirstOrDefault() })
            .OrderBy(r => r.exam.CountryId)
            .ThenBy(r => r.category == null ? null : r.category.Name)
            .ThenBy(r => r.exam.Title)
            .ThenBy(r => r.exam.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AdminExamDto>
        {
            Items = rows.Select(r => AdminExamMapping.ToExamDto(r.exam, r.country.Name, r.category?.Name)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetAdminExamQueryHandler : IRequestHandler<GetAdminExamQuery, AdminExamDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminExamQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamDto> Handle(GetAdminExamQuery request, CancellationToken cancellationToken)
    {
        var row = await AdminExamLoader.LoadExamRowAsync(_context, request.Id, cancellationToken);
        return AdminExamMapping.ToExamDto(row.Exam, row.CountryName, row.CategoryName);
    }
}

public class CreateAdminExamCommandHandler : IRequestHandler<CreateAdminExamCommand, AdminExamDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminExamCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamDto> Handle(CreateAdminExamCommand request, CancellationToken cancellationToken)
    {
        var countryName = await _context.Countries
            .Where(c => c.Id == request.Request.CountryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (countryName is null)
        {
            throw new KeyNotFoundException("Country was not found.");
        }

        await EnsureSlugUniqueAsync(_context, request.Request.Slug, null, cancellationToken);
        await AdminExamContentValidator.EnsureCategoryMatchesCountryAsync(_context, request.Request.ExamCategoryId, request.Request.CountryId, cancellationToken);

        var categoryName = request.Request.ExamCategoryId is null
            ? null
            : await _context.ExamCategories.Where(c => c.Id == request.Request.ExamCategoryId).Select(c => c.Name).FirstAsync(cancellationToken);

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = request.Request.CountryId,
            ExamCategoryId = request.Request.ExamCategoryId,
            Title = request.Request.Title.Trim(),
            Slug = request.Request.Slug.Trim(),
            Description = request.Request.Description?.Trim(),
            Instructions = request.Request.Instructions?.Trim(),
            DurationMinutes = request.Request.DurationMinutes,
            PassingScorePercentage = request.Request.PassingScorePercentage,
            IsFree = request.Request.IsFree,
            Status = ExamStatus.Draft
        };
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync(cancellationToken);
        return AdminExamMapping.ToExamDto(exam, countryName, categoryName);
    }

    internal static async Task EnsureSlugUniqueAsync(
        IApplicationDbContext context,
        string slug,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Exams.AnyAsync(e =>
                e.Slug == slug.Trim()
                && (currentId == null || e.Id != currentId),
            cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("An exam with this slug already exists.");
        }
    }
}

public class UpdateAdminExamCommandHandler : IRequestHandler<UpdateAdminExamCommand, AdminExamDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminExamCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamDto> Handle(UpdateAdminExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        if (exam.Status == ExamStatus.Archived)
        {
            throw new InvalidOperationException("Archived exams cannot be updated.");
        }

        await CreateAdminExamCommandHandler.EnsureSlugUniqueAsync(_context, request.Request.Slug, exam.Id, cancellationToken);
        await AdminExamContentValidator.EnsureCategoryMatchesCountryAsync(_context, request.Request.ExamCategoryId, request.Request.CountryId, cancellationToken);

        var structuralChange =
            exam.CountryId != request.Request.CountryId
            || exam.ExamCategoryId != request.Request.ExamCategoryId
            || exam.DurationMinutes != request.Request.DurationMinutes
            || exam.PassingScorePercentage != request.Request.PassingScorePercentage;

        if (structuralChange)
        {
            var hasLockedVersion = await _context.ExamVersions.AnyAsync(v =>
                    v.ExamId == exam.Id && v.Status != ExamVersionStatus.Draft,
                cancellationToken);
            var hasSessions = await _context.ExamSessions.AnyAsync(s => s.ExamId == exam.Id, cancellationToken);
            if (hasLockedVersion || hasSessions)
            {
                throw new InvalidOperationException("Exam structural and scoring fields cannot be changed after published or retired versions or sessions exist.");
            }
        }

        exam.CountryId = request.Request.CountryId;
        exam.ExamCategoryId = request.Request.ExamCategoryId;
        exam.Title = request.Request.Title.Trim();
        exam.Slug = request.Request.Slug.Trim();
        exam.Description = request.Request.Description?.Trim();
        exam.Instructions = request.Request.Instructions?.Trim();
        exam.DurationMinutes = request.Request.DurationMinutes;
        exam.PassingScorePercentage = request.Request.PassingScorePercentage;
        exam.IsFree = request.Request.IsFree;

        await _context.SaveChangesAsync(cancellationToken);
        var row = await AdminExamLoader.LoadExamRowAsync(_context, exam.Id, cancellationToken);
        return AdminExamMapping.ToExamDto(row.Exam, row.CountryName, row.CategoryName);
    }
}

public class ArchiveAdminExamCommandHandler : IRequestHandler<ArchiveAdminExamCommand, AdminExamDto>
{
    private readonly IApplicationDbContext _context;

    public ArchiveAdminExamCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamDto> Handle(ArchiveAdminExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        exam.Status = ExamStatus.Archived;
        await _context.SaveChangesAsync(cancellationToken);
        var row = await AdminExamLoader.LoadExamRowAsync(_context, exam.Id, cancellationToken);
        return AdminExamMapping.ToExamDto(row.Exam, row.CountryName, row.CategoryName);
    }
}

public class DeleteAdminExamCommandHandler : IRequestHandler<DeleteAdminExamCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminExamCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        var hasVersions = await _context.ExamVersions.AnyAsync(v => v.ExamId == exam.Id, cancellationToken);
        var hasSessions = await _context.ExamSessions.AnyAsync(s => s.ExamId == exam.Id, cancellationToken);
        if (exam.Status != ExamStatus.Draft || hasVersions || hasSessions)
        {
            throw new InvalidOperationException("Only draft exams without versions or sessions can be deleted.");
        }

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

internal static class AdminExamLoader
{
    public static async Task<(Exam Exam, string CountryName, string? CategoryName)> LoadExamRowAsync(
        IApplicationDbContext context,
        Guid examId,
        CancellationToken cancellationToken)
    {
        var row = await context.Exams
            .Where(e => e.Id == examId)
            .Join(context.Countries, e => e.CountryId, c => c.Id, (exam, country) => new { exam, country })
            .GroupJoin(context.ExamCategories, ec => ec.exam.ExamCategoryId, category => category.Id,
                (ec, categories) => new { ec.exam, ec.country, category = categories.FirstOrDefault() })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new KeyNotFoundException("Exam was not found.");
        }

        return (row.exam, row.country.Name, row.category?.Name);
    }
}
