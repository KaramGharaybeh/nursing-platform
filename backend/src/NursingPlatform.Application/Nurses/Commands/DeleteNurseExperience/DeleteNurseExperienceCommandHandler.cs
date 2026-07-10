using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseExperience;

public class DeleteNurseExperienceCommandHandler : IRequestHandler<DeleteNurseExperienceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public DeleteNurseExperienceCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task Handle(DeleteNurseExperienceCommand command, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var experience = await _context.NurseExperiences
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.NurseProfileId == profile.Id, cancellationToken);

        if (experience is null)
        {
            throw new KeyNotFoundException("Nurse experience was not found.");
        }

        _context.NurseExperiences.Remove(experience);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
