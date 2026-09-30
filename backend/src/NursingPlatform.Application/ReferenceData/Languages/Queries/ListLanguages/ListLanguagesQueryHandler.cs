using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.ReferenceData.Languages.DTOs;

namespace NursingPlatform.Application.ReferenceData.Languages.Queries.ListLanguages;

public class ListLanguagesQueryHandler : IRequestHandler<ListLanguagesQuery, IReadOnlyList<LanguageListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListLanguagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LanguageListItemDto>> Handle(ListLanguagesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new LanguageListItemDto
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code
            })
            .ToListAsync(cancellationToken);
    }
}
