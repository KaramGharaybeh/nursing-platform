using MediatR;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;

public class CreateContactRequestCommand : IRequest<ContactRequestDto>
{
    public Guid NurseProfileId { get; init; }
}
