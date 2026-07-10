using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Queries.GetCurrentNurseCv;

public record GetCurrentNurseCvQuery : IRequest<NurseCvDocumentDto>;
