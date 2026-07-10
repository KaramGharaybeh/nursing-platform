using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Employers.DTOs;
using NursingPlatform.Domain.Employers;

namespace NursingPlatform.Application.Employers.Commands.UpsertMyEmployerProfile;

public class UpsertMyEmployerProfileCommandHandler : IRequestHandler<UpsertMyEmployerProfileCommand, EmployerProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly EmployerRoleGuard _employerRoleGuard;

    public UpsertMyEmployerProfileCommandHandler(
        IApplicationDbContext context,
        EmployerRoleGuard employerRoleGuard)
    {
        _context = context;
        _employerRoleGuard = employerRoleGuard;
    }

    public async Task<EmployerProfileDto> Handle(UpsertMyEmployerProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = await _employerRoleGuard.EnsureEmployerAsync(cancellationToken);

        var profile = await _context.EmployerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new EmployerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };
            _context.EmployerProfiles.Add(profile);
        }

        profile.JobTitle = NormalizeOptional(command.JobTitle);
        profile.Department = NormalizeOptional(command.Department);

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(profile);
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static EmployerProfileDto ToDto(EmployerProfile profile)
    {
        return new EmployerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            JobTitle = profile.JobTitle,
            Department = profile.Department
        };
    }
}
