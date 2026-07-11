using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;

public class CreateContactRequestCommandHandler : IRequestHandler<CreateContactRequestCommand, ContactRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public CreateContactRequestCommandHandler(IApplicationDbContext context, EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<ContactRequestDto> Handle(CreateContactRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var employerProfile = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id, p.JobTitle, p.Department })
            .FirstOrDefaultAsync(cancellationToken);

        if (employerProfile is null)
        {
            throw new ForbiddenAccessException("Employer profile is required before requesting contact.");
        }

        var organization = await _context.EmployerOrganizations
            .Where(o => o.EmployerProfileId == employerProfile.Id)
            .Select(o => new { o.Id, o.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (organization is null)
        {
            throw new ForbiddenAccessException("Employer organization is required before requesting contact.");
        }

        var candidate = await _context.NurseProfiles
            .Where(p => p.Id == request.NurseProfileId
                && p.IsAvailableForRecruitment
                && p.User.IsActive
                && p.User.EmailVerified)
            .Select(p => new
            {
                p.Id,
                p.Headline,
                LicenseCountryName = p.LicenseCountry == null ? null : p.LicenseCountry.Name,
                CurrentCountryName = p.CurrentCountry == null ? null : p.CurrentCountry.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            throw new KeyNotFoundException("Contact request target was not found.");
        }

        var hasActiveDuplicate = await _context.ContactRequests
            .AnyAsync(r => r.EmployerProfileId == employerProfile.Id
                && r.NurseProfileId == request.NurseProfileId
                && (r.Status == ContactRequestStatus.Pending || r.Status == ContactRequestStatus.Approved),
                cancellationToken);

        if (hasActiveDuplicate)
        {
            throw new InvalidOperationException("An active contact request already exists for this candidate.");
        }

        var contactRequest = new ContactRequest
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = employerProfile.Id,
            EmployerOrganizationId = organization.Id,
            NurseProfileId = candidate.Id,
            Status = ContactRequestStatus.Pending,
            CandidateHeadlineSnapshot = candidate.Headline,
            CandidateLicenseCountryNameSnapshot = candidate.LicenseCountryName,
            CandidateCurrentCountryNameSnapshot = candidate.CurrentCountryName,
            EmployerOrganizationNameSnapshot = organization.Name,
            JobTitleSnapshot = employerProfile.JobTitle,
            DepartmentSnapshot = employerProfile.Department
        };

        _context.ContactRequests.Add(contactRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return ContactRequestMapping.ToEmployerDto(contactRequest);
    }
}
