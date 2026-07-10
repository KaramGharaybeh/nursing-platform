using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

public class UpdateNurseSkillsCommandHandler : IRequestHandler<UpdateNurseSkillsCommand, IReadOnlyList<NurseSkillDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpdateNurseSkillsCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseSkillDto>> Handle(UpdateNurseSkillsCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var existingSkills = await _context.NurseSkills
            .Where(s => s.NurseProfileId == profile.Id)
            .ToListAsync(cancellationToken);

        foreach (var existingSkill in existingSkills)
        {
            _context.NurseSkills.Remove(existingSkill);
        }

        var createdSkills = command.Skills
            .Select(skill => SkillNameNormalizer.NormalizeName(skill))
            .Select(skill => new NurseSkill
            {
                Id = Guid.NewGuid(),
                NurseProfileId = profile.Id,
                Name = skill,
                NormalizedName = SkillNameNormalizer.NormalizeForComparison(skill)
            })
            .ToList();

        foreach (var skill in createdSkills)
        {
            _context.NurseSkills.Add(skill);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdSkills
            .OrderBy(skill => skill.Name)
            .Select(skill => new NurseSkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            })
            .ToList();
    }

    private async Task<NurseProfile> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        return profile;
    }
}
