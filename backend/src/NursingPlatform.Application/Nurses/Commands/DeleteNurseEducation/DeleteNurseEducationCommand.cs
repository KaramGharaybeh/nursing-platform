using MediatR;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseEducation;

public record DeleteNurseEducationCommand(Guid Id) : IRequest;
