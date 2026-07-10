using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseCertificate;

public class DeleteNurseCertificateCommandHandler : IRequestHandler<DeleteNurseCertificateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public DeleteNurseCertificateCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task Handle(DeleteNurseCertificateCommand command, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var certificate = await _context.NurseCertificates
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.NurseProfileId == profile.Id, cancellationToken);

        if (certificate is null)
        {
            throw new KeyNotFoundException("Nurse certificate was not found.");
        }

        _context.NurseCertificates.Remove(certificate);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
