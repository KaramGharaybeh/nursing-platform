using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;

public class ListCurrentNurseLanguagesQueryHandler : IRequestHandler<ListCurrentNurseLanguagesQuery, IReadOnlyList<NurseLanguageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListCurrentNurseLanguagesQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseLanguageDto>> Handle(ListCurrentNurseLanguagesQuery request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var nurseLanguages = await _context.NurseLanguages
            .Where(l => l.NurseProfileId == profile.Id)
            .Select(l => new
            {
                l.Id,
                l.LanguageId,
                l.Proficiency
            })
            .ToListAsync(cancellationToken);

        var languageIds = nurseLanguages.Select(l => l.LanguageId).Distinct().ToList();
        var languages = await _context.Languages
            .Where(l => languageIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        return nurseLanguages
            .Where(l => languages.ContainsKey(l.LanguageId))
            .Select(l => new NurseLanguageDto
            {
                Id = l.Id,
                LanguageId = l.LanguageId,
                Name = languages[l.LanguageId].Name,
                Code = languages[l.LanguageId].Code,
                Proficiency = l.Proficiency
            })
            .OrderBy(l => l.Name)
            .ToList();
    }
}
