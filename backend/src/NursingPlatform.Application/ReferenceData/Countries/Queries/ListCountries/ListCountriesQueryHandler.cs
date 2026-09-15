using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.ReferenceData.Countries.DTOs;

namespace NursingPlatform.Application.ReferenceData.Countries.Queries.ListCountries;

public class ListCountriesQueryHandler : IRequestHandler<ListCountriesQuery, IReadOnlyList<CountryListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCountriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CountryListItemDto>> Handle(ListCountriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CountryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            })
            .ToListAsync(cancellationToken);
    }
}
