using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;

public class GetCurrentNurseCvQueryHandler : IRequestHandler<GetCurrentNurseCvQuery, NurseCvDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public GetCurrentNurseCvQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseCvDocumentDto> Handle(GetCurrentNurseCvQuery request, CancellationToken cancellationToken)
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

        var cvDocument = await _context.NurseCvDocuments
            .Where(c => c.NurseProfileId == profile.Id)
            .Select(c => new NurseCvDocumentDto
            {
                Id = c.Id,
                FileName = c.OriginalFileName,
                ContentType = c.ContentType,
                FileSizeBytes = c.FileSizeBytes,
                UploadedAt = c.UploadedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cvDocument is null)
        {
            throw new KeyNotFoundException("Nurse CV was not found.");
        }

        return cvDocument;
    }
}
