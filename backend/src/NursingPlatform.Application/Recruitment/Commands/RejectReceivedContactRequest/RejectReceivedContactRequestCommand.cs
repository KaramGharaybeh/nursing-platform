using MediatR;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;

public class RejectReceivedContactRequestCommand : IRequest<ReceivedContactRequestDto>
{
    public Guid Id { get; init; }
}
