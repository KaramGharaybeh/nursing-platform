using MediatR;
using NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseExperience;

public class UpdateNurseExperienceCommand : UpsertNurseExperienceRequest, IRequest<NurseExperienceDto>
{
    public Guid Id { get; init; }
}

public class UpdateNurseExperienceCommandValidator : UpsertNurseExperienceRequestValidator<UpdateNurseExperienceCommand>;
