using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;

public class ListCurrentNurseCertificatesQueryHandler : IRequestHandler<ListCurrentNurseCertificatesQuery, IReadOnlyList<NurseCertificateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ListCurrentNurseCertificatesQueryHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseCertificateDto>> Handle(ListCurrentNurseCertificatesQuery request, CancellationToken cancellationToken)
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

        return await _context.NurseCertificates
            .Where(c => c.NurseProfileId == profile.Id)
            .OrderByDescending(c => c.IssueDate)
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new NurseCertificateDto
            {
                Id = c.Id,
                Name = c.Name,
                IssuingOrganization = c.IssuingOrganization,
                IssueDate = c.IssueDate,
                ExpirationDate = c.ExpirationDate,
                CredentialId = c.CredentialId,
                CredentialUrl = c.CredentialUrl
            })
            .ToListAsync(cancellationToken);
    }
}
