using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseExperiences;

public record ListCurrentNurseExperiencesQuery : IRequest<IReadOnlyList<NurseExperienceDto>>;
