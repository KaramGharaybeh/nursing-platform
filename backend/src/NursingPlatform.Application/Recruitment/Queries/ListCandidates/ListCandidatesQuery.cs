using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Recruitment.Queries.ListCandidates;

public class ListCandidatesQuery : IRequest<PaginatedResult<CandidateListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? LicenseCountryId { get; init; }
    public Guid? CurrentCountryId { get; init; }
    public int? MinimumYearsOfExperience { get; init; }
    public IReadOnlyCollection<string> Skills { get; init; } = [];
    public Guid? LanguageId { get; init; }
}
