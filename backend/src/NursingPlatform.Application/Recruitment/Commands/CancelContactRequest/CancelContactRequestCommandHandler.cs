using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;

public class CancelContactRequestCommandHandler : IRequestHandler<CancelContactRequestCommand, ContactRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public CancelContactRequestCommandHandler(IApplicationDbContext context, EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<ContactRequestDto> Handle(CancelContactRequestCommand request, CancellationToken cancellationToken)
    {
        var employerProfileId = await GetEmployerProfileIdAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var affectedRows = await _context.ExecuteContactRequestTransitionAsync(
            request.Id,
            employerProfileId,
            isEmployerOwner: true,
            ContactRequestStatus.Cancelled,
            now,
            cancellationToken);

        if (affectedRows == 0)
        {
            await ThrowNotFoundOrConflictAsync(request.Id, employerProfileId, cancellationToken);
        }

        var updated = await _context.ContactRequests
            .Where(r => r.Id == request.Id && r.EmployerProfileId == employerProfileId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Contact request was not found.");

        return ContactRequestMapping.ToEmployerDto(updated);
    }

    private async Task<Guid> GetEmployerProfileIdAsync(CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);
        var employerProfileId = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return employerProfileId
            ?? throw new ForbiddenAccessException("Employer profile is required before managing contact requests.");
    }

    private async Task ThrowNotFoundOrConflictAsync(Guid id, Guid employerProfileId, CancellationToken cancellationToken)
    {
        var status = await _context.ContactRequests
            .Where(r => r.Id == id && r.EmployerProfileId == employerProfileId)
            .Select(r => (ContactRequestStatus?)r.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (status is null)
        {
            throw new KeyNotFoundException("Contact request was not found.");
        }

        throw new InvalidOperationException("Only pending contact requests can be cancelled.");
    }
}
