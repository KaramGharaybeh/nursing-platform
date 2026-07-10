using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseLanguages;

public record ListCurrentNurseLanguagesQuery : IRequest<IReadOnlyList<NurseLanguageDto>>;
