using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseCv;

public class DeleteNurseCvCommandHandler : IRequestHandler<DeleteNurseCvCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;
    private readonly IFileStorageService _fileStorage;

    public DeleteNurseCvCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        IFileStorageService fileStorage)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteNurseCvCommand request, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var cvDocument = await _context.NurseCvDocuments
            .FirstOrDefaultAsync(c => c.NurseProfileId == profile.Id, cancellationToken);

        if (cvDocument is null)
        {
            throw new KeyNotFoundException("Nurse CV was not found.");
        }

        await _fileStorage.DeleteAsync(cvDocument.StorageKey, cancellationToken);
        _context.NurseCvDocuments.Remove(cvDocument);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
