using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;

public class CreateNurseEducationCommand : UpsertNurseEducationRequest, IRequest<NurseEducationDto>;
