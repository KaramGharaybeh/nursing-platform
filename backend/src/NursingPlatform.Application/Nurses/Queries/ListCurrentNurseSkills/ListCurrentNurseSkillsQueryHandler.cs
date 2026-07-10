using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;

public class ListCurrentNurseSkillsQueryHandler : IRequestHandler<ListCurrentNurseSkillsQuery, IReadOnlyList<NurseSkillDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListCurrentNurseSkillsQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseSkillDto>> Handle(ListCurrentNurseSkillsQuery request, CancellationToken cancellationToken)
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

        return await _context.NurseSkills
            .Where(s => s.NurseProfileId == profile.Id)
            .OrderBy(s => s.Name)
            .Select(s => new NurseSkillDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync(cancellationToken);
    }
}
