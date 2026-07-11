using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;

public class ListMyContactRequestsQuery : IRequest<PaginatedResult<ContactRequestDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ContactRequestStatus? Status { get; init; }
}
