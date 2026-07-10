using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseSkills;

public record ListCurrentNurseSkillsQuery : IRequest<IReadOnlyList<NurseSkillDto>>;
