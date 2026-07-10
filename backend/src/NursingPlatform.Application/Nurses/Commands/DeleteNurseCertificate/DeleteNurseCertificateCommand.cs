using MediatR;

namespace NursingPlatform.Application.Nurses.Commands.DeleteNurseCertificate;

public record DeleteNurseCertificateCommand(Guid Id) : IRequest;
