using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.ListCurrentNurseCertificates;

public record ListCurrentNurseCertificatesQuery : IRequest<IReadOnlyList<NurseCertificateDto>>;
