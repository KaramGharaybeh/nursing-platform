using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseExperience;

public class CreateNurseExperienceCommand : UpsertNurseExperienceRequest, IRequest<NurseExperienceDto>;
