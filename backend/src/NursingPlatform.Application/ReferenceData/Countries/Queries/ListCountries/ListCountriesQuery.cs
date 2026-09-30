using MediatR;
using NursingPlatform.Application.ReferenceData.Countries.DTOs;

namespace NursingPlatform.Application.ReferenceData.Countries.Queries.ListCountries;

public record ListCountriesQuery : IRequest<IReadOnlyList<CountryListItemDto>>;
