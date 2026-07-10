using FluentValidation;
using MediatR;
using NursingPlatform.Application.Nurses.Commands.CreateNurseEducation;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseEducation;

public class UpdateNurseEducationCommand : UpsertNurseEducationRequest, IRequest<NurseEducationDto>
{
    public Guid Id { get; init; }
}

public class UpdateNurseEducationCommandValidator : AbstractValidator<UpdateNurseEducationCommand>
{
    public UpdateNurseEducationCommandValidator()
    {
        Include(new UpsertNurseEducationRequestValidator<UpdateNurseEducationCommand>());
    }
}
