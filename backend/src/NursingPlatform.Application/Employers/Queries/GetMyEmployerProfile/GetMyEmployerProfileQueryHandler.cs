using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;

namespace NursingPlatform.Application.Employers.Queries.GetMyEmployerProfile;

public class GetMyEmployerProfileQueryHandler : IRequestHandler<GetMyEmployerProfileQuery, EmployerProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public GetMyEmployerProfileQueryHandler(
        IApplicationDbContext context,
        EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<EmployerProfileDto> Handle(GetMyEmployerProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);

        var profile = await _context.EmployerProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new EmployerProfileDto
            {
                Id = p.Id,
                UserId = p.UserId,
                JobTitle = p.JobTitle,
                Department = p.Department
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Employer profile was not found.");
        }

        return profile;
    }
}
