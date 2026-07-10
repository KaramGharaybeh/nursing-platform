using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;

public class CreateNurseCertificateCommandHandler : IRequestHandler<CreateNurseCertificateCommand, NurseCertificateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public CreateNurseCertificateCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<NurseCertificateDto> Handle(CreateNurseCertificateCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var certificate = new NurseCertificate
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            Name = command.Name,
            IssuingOrganization = command.IssuingOrganization,
            IssueDate = command.IssueDate,
            ExpirationDate = command.ExpirationDate,
            CredentialId = command.CredentialId,
            CredentialUrl = command.CredentialUrl
        };

        _context.NurseCertificates.Add(certificate);
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
