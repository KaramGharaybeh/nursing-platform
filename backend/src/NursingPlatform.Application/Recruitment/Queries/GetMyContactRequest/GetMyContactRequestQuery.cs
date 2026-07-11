using MediatR;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;

public class GetMyContactRequestQuery : IRequest<ContactRequestDto>
{
    public Guid Id { get; init; }
}
