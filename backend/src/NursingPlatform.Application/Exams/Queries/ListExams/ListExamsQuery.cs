using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Exams.Queries.ListExams;

public class ListExamsQuery : IRequest<PaginatedResult<ExamCatalogItemDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CountryId { get; set; }
    public Guid? CategoryId { get; set; }
}
