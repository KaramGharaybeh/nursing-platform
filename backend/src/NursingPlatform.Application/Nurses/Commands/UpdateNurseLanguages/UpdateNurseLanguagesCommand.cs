using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;

public class UpdateNurseLanguagesCommand : IRequest<IReadOnlyList<NurseLanguageDto>>
{
    public IReadOnlyList<UpdateNurseLanguageRequest> Languages { get; init; } = [];
}
