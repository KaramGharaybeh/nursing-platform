using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseEducation;

public class DeleteNurseEducationCommandHandler : IRequestHandler<DeleteNurseEducationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public DeleteNurseEducationCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task Handle(DeleteNurseEducationCommand command, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var education = await _context.NurseEducation
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.NurseProfileId == profile.Id, cancellationToken);

        if (education is null)
        {
            throw new KeyNotFoundException("Nurse education was not found.");
        }

        _context.NurseEducation.Remove(education);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
