using MediatR;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Recruitment.Common;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;

public class RejectReceivedContactRequestCommandHandler : IRequestHandler<RejectReceivedContactRequestCommand, ReceivedContactRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public RejectReceivedContactRequestCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<ReceivedContactRequestDto> Handle(RejectReceivedContactRequestCommand request, CancellationToken cancellationToken)
    {
        return await ReceivedContactRequestTransition.ApplyAsync(
            _context,
            _nurseRoleGuard,
            request.Id,
            ContactRequestStatus.Rejected,
            "Only pending contact requests can be rejected.",
            cancellationToken);
    }
}
