using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseEducation;

public record ListCurrentNurseEducationQuery : IRequest<IReadOnlyList<NurseEducationDto>>;
