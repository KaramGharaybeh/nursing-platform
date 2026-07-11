using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

public class ListCandidatesQuery : IRequest<PaginatedResult<CandidateListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
