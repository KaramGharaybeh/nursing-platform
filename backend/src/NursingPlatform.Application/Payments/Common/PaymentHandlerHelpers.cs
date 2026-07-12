using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;

namespace NursingPlatform.Application.Payments.Common;

internal static class PaymentHandlerHelpers
{
    public static async Task<Guid> GetCurrentNurseProfileIdAsync(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        CancellationToken cancellationToken)
    {
        var userId = await nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var nurseProfileId = await context.NurseProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (nurseProfileId is null)
        {
            throw new ForbiddenAccessException("Nurse profile is required before using payment orders.");
        }

        return nurseProfileId.Value;
    }
}
