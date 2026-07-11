using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;

public class ApproveReceivedContactRequestCommandHandler : IRequestHandler<ApproveReceivedContactRequestCommand, ReceivedContactRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public ApproveReceivedContactRequestCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ReceivedContactRequestDto> Handle(ApproveReceivedContactRequestCommand request, CancellationToken cancellationToken)
    {
        return await ReceivedContactRequestTransition.ApplyAsync(
            _context,
            _nurseRoleGuard,
            request.Id,
            ContactRequestStatus.Approved,
            "Only pending contact requests can be approved.",
            cancellationToken);
    }
}
