using MediatR;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;

public class ApproveReceivedContactRequestCommand : IRequest<ReceivedContactRequestDto>
{
    public Guid Id { get; init; }
}
