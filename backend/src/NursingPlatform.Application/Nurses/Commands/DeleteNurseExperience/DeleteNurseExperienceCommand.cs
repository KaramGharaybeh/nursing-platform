using MediatR;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseExperience;

public record DeleteNurseExperienceCommand(Guid Id) : IRequest;
