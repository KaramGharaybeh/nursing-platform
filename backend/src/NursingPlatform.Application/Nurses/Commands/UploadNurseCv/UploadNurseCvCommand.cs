using MediatR;
using NursingPlatform.Application.Nurses.DTOs;

namespace NursingPlatform.Application.Nurses.Commands.UploadNurseCv;

public record UploadNurseCvCommand : IRequest<NurseCvDocumentDto>
{
    public Stream File { get; init; } = Stream.Null;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
}
