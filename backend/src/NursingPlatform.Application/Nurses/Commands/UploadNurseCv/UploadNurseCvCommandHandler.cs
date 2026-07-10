using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Storage;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;

namespace NursingPlatform.Application.Nurses.Commands.UploadNurseCv;

public class UploadNurseCvCommandHandler : IRequestHandler<UploadNurseCvCommand, NurseCvDocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;
    private readonly IFileStorageService _fileStorage;

    public UploadNurseCvCommandHandler(
        IApplicationDbContext context,
        NurseRoleGuard nurseRoleGuard,
        IFileStorageService fileStorage)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
        _fileStorage = fileStorage;
    }

    public async Task<NurseCvDocumentDto> Handle(UploadNurseCvCommand command, CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        var extension = Path.GetExtension(command.OriginalFileName).ToLowerInvariant();
        var storedFile = await _fileStorage.SaveAsync(command.File, extension, command.ContentType, cancellationToken);
        var existingCv = await _context.NurseCvDocuments
            .FirstOrDefaultAsync(c => c.NurseProfileId == profile.Id, cancellationToken);
        var oldStorageKey = existingCv?.StorageKey;

        if (existingCv is not null)
        {
            _context.NurseCvDocuments.Remove(existingCv);
        }

        var cvDocument = new NurseCvDocument
        {
            Id = Guid.NewGuid(),
            NurseProfileId = profile.Id,
            OriginalFileName = SanitizeFileName(command.OriginalFileName, extension),
            StorageKey = storedFile.StorageKey,
            ContentType = storedFile.ContentType,
            FileSizeBytes = storedFile.FileSizeBytes,
            UploadedAt = DateTime.UtcNow
        };

        _context.NurseCvDocuments.Add(cvDocument);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TryDeleteAsync(storedFile.StorageKey, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldStorageKey))
        {
            await _fileStorage.DeleteAsync(oldStorageKey, cancellationToken);
        }

        return MapToDto(cvDocument);
    }

    private static NurseCvDocumentDto MapToDto(NurseCvDocument cvDocument)
    {
        return new NurseCvDocumentDto
        {
            Id = cvDocument.Id,
            FileName = cvDocument.OriginalFileName,
            ContentType = cvDocument.ContentType,
            FileSizeBytes = cvDocument.FileSizeBytes,
            UploadedAt = cvDocument.UploadedAt
        };
    }

    private static string SanitizeFileName(string originalFileName, string extension)
    {
        var fileName = Path.GetFileName(originalFileName.Replace('\\', '/'));

        return string.IsNullOrWhiteSpace(fileName) ? $"cv{extension}" : fileName;
    }

    private async Task TryDeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            await _fileStorage.DeleteAsync(storageKey, cancellationToken);
        }
        catch
        {
            // Preserve the original persistence failure; cleanup is best effort.
        }
    }
}
