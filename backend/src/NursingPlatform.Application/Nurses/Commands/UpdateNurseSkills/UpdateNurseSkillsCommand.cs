using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills;

public class UpdateNurseSkillsCommand : IRequest<IReadOnlyList<NurseSkillDto>>
{
    public IReadOnlyList<string> Skills { get; init; } = [];
}
