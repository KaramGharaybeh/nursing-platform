using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

public class ListCandidatesQueryHandler : IRequestHandler<ListCandidatesQuery, PaginatedResult<CandidateListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public ListCandidatesQueryHandler(
        IApplicationDbContext context,
        EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<PaginatedResult<CandidateListItemDto>> Handle(ListCandidatesQuery request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var employerProfile = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (employerProfile is null)
        {
            throw new ForbiddenAccessException("Employer profile is required before searching candidates.");
        }

        var hasOrganization = await _context.EmployerOrganizations
            .AnyAsync(o => o.EmployerProfileId == employerProfile.Id, cancellationToken);

        if (!hasOrganization)
        {
            throw new ForbiddenAccessException("Employer organization is required before searching candidates.");
        }

        var eligibleCandidates = _context.NurseProfiles
            .Where(p => p.IsAvailableForRecruitment && p.User.IsActive && p.User.EmailVerified);

        if (request.LicenseCountryId.HasValue)
        {
            eligibleCandidates = eligibleCandidates.Where(p => p.LicenseCountryId == request.LicenseCountryId.Value);
        }

        if (request.CurrentCountryId.HasValue)
        {
            eligibleCandidates = eligibleCandidates.Where(p => p.CurrentCountryId == request.CurrentCountryId.Value);
        }

        if (request.MinimumYearsOfExperience.HasValue)
        {
            eligibleCandidates = eligibleCandidates.Where(p => p.YearsOfExperience >= request.MinimumYearsOfExperience.Value);
        }

        if (request.LanguageId.HasValue)
        {
            eligibleCandidates = eligibleCandidates.Where(p =>
                _context.NurseLanguages.Any(l => l.NurseProfileId == p.Id && l.LanguageId == request.LanguageId.Value));
        }

        var normalizedSkillNames = CandidateSkillFilterParser.ParseNormalizedNames(request.Skills);
        foreach (var normalizedSkillName in normalizedSkillNames)
        {
            var skillName = normalizedSkillName;
            eligibleCandidates = eligibleCandidates.Where(p =>
                _context.NurseSkills.Any(s => s.NurseProfileId == p.Id && s.NormalizedName == skillName));
        }

        var totalCount = await eligibleCandidates.CountAsync(cancellationToken);
        var candidatePage = await eligibleCandidates
            .OrderByDescending(p => p.YearsOfExperience)
            .ThenByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new CandidatePageItem(
                p.Id,
                p.Headline,
                p.ProfessionalSummary,
                p.LicenseCountryId,
                p.CurrentCountryId,
                p.YearsOfExperience))
            .ToListAsync(cancellationToken);

        if (candidatePage.Count == 0)
        {
            return new PaginatedResult<CandidateListItemDto>
            {
                Items = [],
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        var profileIds = candidatePage.Select(c => c.NurseProfileId).ToList();
        var countryIds = candidatePage
            .SelectMany(c => new[] { c.LicenseCountryId, c.CurrentCountryId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var countries = await _context.Countries
            .Where(c => countryIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);
        var countryNamesById = countries.ToDictionary(c => c.Id, c => c.Name);

        var skills = await _context.NurseSkills
            .Where(s => profileIds.Contains(s.NurseProfileId))
            .Select(s => new { s.NurseProfileId, s.Name })
            .ToListAsync(cancellationToken);
        var skillsByProfileId = skills
            .GroupBy(s => s.NurseProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => s.Name).Distinct().OrderBy(name => name).ToList());

        var languages = await _context.NurseLanguages
            .Where(l => profileIds.Contains(l.NurseProfileId))
            .Select(l => new
            {
                l.NurseProfileId,
                l.Language.Name,
                l.Language.Code,
                l.Proficiency
            })
            .ToListAsync(cancellationToken);
        var languagesByProfileId = languages
            .GroupBy(l => l.NurseProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => l.Name)
                    .ThenBy(l => l.Code)
                    .Select(l => new CandidateLanguageDto
                    {
                        Name = l.Name,
                        Code = l.Code,
                        Proficiency = l.Proficiency
                    })
                    .ToList());

        var certificates = await _context.NurseCertificates
            .Where(c => profileIds.Contains(c.NurseProfileId))
            .Select(c => new { c.NurseProfileId })
            .ToListAsync(cancellationToken);
        var certificateCountsByProfileId = certificates
            .GroupBy(c => c.NurseProfileId)
            .ToDictionary(g => g.Key, g => g.Count());

        var experiences = await _context.NurseExperiences
            .Where(e => profileIds.Contains(e.NurseProfileId))
            .Select(e => new { e.NurseProfileId, e.JobTitle, e.IsCurrent, e.StartDate, e.Id })
            .ToListAsync(cancellationToken);
        var latestExperienceTitleByProfileId = experiences
            .GroupBy(e => e.NurseProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e => e.IsCurrent)
                    .ThenByDescending(e => e.StartDate)
                    .ThenBy(e => e.Id)
                    .Select(e => e.JobTitle)
                    .FirstOrDefault());

        var education = await _context.NurseEducation
            .Where(e => profileIds.Contains(e.NurseProfileId))
            .Select(e => new { e.NurseProfileId, e.Degree, e.FieldOfStudy, e.EndDate, e.StartDate, e.Id })
            .ToListAsync(cancellationToken);
        var educationSummaryByProfileId = education
            .GroupBy(e => e.NurseProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e => e.EndDate)
                    .ThenByDescending(e => e.StartDate)
                    .ThenBy(e => e.Id)
                    .Select(e => FormatEducationSummary(e.Degree, e.FieldOfStudy))
                    .FirstOrDefault());

        var items = candidatePage.Select(candidate =>
        {
            var certificatesCount = certificateCountsByProfileId.GetValueOrDefault(candidate.NurseProfileId);

            return new CandidateListItemDto
            {
                NurseProfileId = candidate.NurseProfileId,
                Headline = candidate.Headline,
                ProfessionalSummary = candidate.ProfessionalSummary,
                LicenseCountryName = candidate.LicenseCountryId.HasValue
                    ? countryNamesById.GetValueOrDefault(candidate.LicenseCountryId.Value)
                    : null,
                CurrentCountryName = candidate.CurrentCountryId.HasValue
                    ? countryNamesById.GetValueOrDefault(candidate.CurrentCountryId.Value)
                    : null,
                YearsOfExperience = candidate.YearsOfExperience,
                Skills = skillsByProfileId.GetValueOrDefault(candidate.NurseProfileId) ?? [],
                Languages = languagesByProfileId.GetValueOrDefault(candidate.NurseProfileId) ?? [],
                CertificatesSummary = FormatCertificatesSummary(certificatesCount),
                CertificatesCount = certificatesCount,
                LatestExperienceTitle = latestExperienceTitleByProfileId.GetValueOrDefault(candidate.NurseProfileId),
                EducationSummary = educationSummaryByProfileId.GetValueOrDefault(candidate.NurseProfileId)
            };
        }).ToList();

        return new PaginatedResult<CandidateListItemDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static string FormatCertificatesSummary(int count)
    {
        return count == 1 ? "1 certificate" : $"{count} certificates";
    }

    private static string FormatEducationSummary(string degree, string? fieldOfStudy)
    {
        return string.IsNullOrWhiteSpace(fieldOfStudy) ? degree : $"{degree} of {fieldOfStudy}";
    }

    private sealed record CandidatePageItem(
        Guid NurseProfileId,
        string? Headline,
        string? ProfessionalSummary,
        Guid? LicenseCountryId,
        Guid? CurrentCountryId,
        int YearsOfExperience);
}
