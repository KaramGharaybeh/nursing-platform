using MediatR;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;

public class CancelContactRequestCommand : IRequest<ContactRequestDto>
{
    public Guid Id { get; init; }
}
