using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.CreateNurseCertificate;

public class CreateNurseCertificateCommand : UpsertNurseCertificateRequest, IRequest<NurseCertificateDto>;
