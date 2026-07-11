using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Common;

internal static class ReceivedContactRequestTransition
{
    public static async Task<ReceivedContactRequestDto> ApplyAsync(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        Guid id,
        ContactRequestStatus status,
        string conflictMessage,
        CancellationToken cancellationToken)
    {
        var nurseProfileId = await GetNurseProfileIdAsync(context, nurseRoleGuard, cancellationToken);
        var affectedRows = await context.ExecuteContactRequestTransitionAsync(
            id,
            nurseProfileId,
            isEmployerOwner: false,
            status,
            DateTime.UtcNow,
            cancellationToken);

        if (affectedRows == 0)
        {
            await ThrowNotFoundOrConflictAsync(context, id, nurseProfileId, conflictMessage, cancellationToken);
        }

        var updated = await context.ContactRequests
            .Where(r => r.Id == id && r.NurseProfileId == nurseProfileId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Contact request was not found.");

        return ContactRequestMapping.ToNurseDto(updated);
    }

    private static async Task<Guid> GetNurseProfileIdAsync(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        CancellationToken cancellationToken)
    {
        var userId = await nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return nurseProfileId
            ?? throw new ForbiddenAccessException("Nurse profile is required before managing received contact requests.");
    }

    private static async Task ThrowNotFoundOrConflictAsync(
        IApplicationDbContext context,
        Guid id,
        Guid nurseProfileId,
        string conflictMessage,
        CancellationToken cancellationToken)
    {
        var status = await context.ContactRequests
            .Where(r => r.Id == id && r.NurseProfileId == nurseProfileId)
            .Select(r => (ContactRequestStatus?)r.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (status is null)
        {
            throw new KeyNotFoundException("Contact request was not found.");
        }

        throw new InvalidOperationException(conflictMessage);
    }
}
