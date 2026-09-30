using MediatR;
using NursingPlatform.Application.ReferenceData.Languages.DTOs;

namespace NursingPlatform.Application.ReferenceData.Languages.Queries.ListLanguages;

public record ListLanguagesQuery : IRequest<IReadOnlyList<LanguageListItemDto>>;
