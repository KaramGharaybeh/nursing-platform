using FluentValidation;
using MediatR;
using NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseCertificate;

public class UpdateNurseCertificateCommand : UpsertNurseCertificateRequest, IRequest<NurseCertificateDto>
{
    public Guid Id { get; init; }
}

public class UpdateNurseCertificateCommandValidator : AbstractValidator<UpdateNurseCertificateCommand>
{
    public UpdateNurseCertificateCommandValidator()
    {
        Include(new UpsertNurseCertificateRequestValidator<UpdateNurseCertificateCommand>());
    }
}
