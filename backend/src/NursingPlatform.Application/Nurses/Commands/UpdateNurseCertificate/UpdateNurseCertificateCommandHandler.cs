using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseCertificate;

public class UpdateNurseCertificateCommandHandler : IRequestHandler<UpdateNurseCertificateCommand, NurseCertificateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpdateNurseCertificateCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseCertificateDto> Handle(UpdateNurseCertificateCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var certificate = await _context.NurseCertificates
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.NurseProfileId == profile.Id, cancellationToken);

        if (certificate is null)
        {
            throw new KeyNotFoundException("Nurse certificate was not found.");
        }

        certificate.Name = command.Name;
        certificate.IssuingOrganization = command.IssuingOrganization;
        certificate.IssueDate = command.IssueDate;
        certificate.ExpirationDate = command.ExpirationDate;
        certificate.CredentialId = command.CredentialId;
        certificate.CredentialUrl = command.CredentialUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(certificate);
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

    private static NurseCertificateDto ToDto(NurseCertificate certificate)
    {
        return new NurseCertificateDto
        {
            Id = certificate.Id,
            Name = certificate.Name,
            IssuingOrganization = certificate.IssuingOrganization,
            IssueDate = certificate.IssueDate,
            ExpirationDate = certificate.ExpirationDate,
            CredentialId = certificate.CredentialId,
            CredentialUrl = certificate.CredentialUrl
        };
    }
}
