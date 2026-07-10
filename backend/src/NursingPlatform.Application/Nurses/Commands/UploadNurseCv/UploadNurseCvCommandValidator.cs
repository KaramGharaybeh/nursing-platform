using FluentValidation;

namespace NursingPlatform.Application.Nurses.Commands.UploadNurseCv;

public class UploadNurseCvCommandValidator : AbstractValidator<UploadNurseCvCommand>
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx"
    };

    public UploadNurseCvCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(file => file != Stream.Null)
            .WithMessage("CV file is required.");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage("Original file name is required.")
            .Must(HasAllowedExtension)
            .WithMessage("CV file extension is not supported.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.")
            .Must(contentType => AllowedContentTypes.Contains(contentType))
            .WithMessage("CV content type is not supported.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("CV file must not be empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("CV file must be 5 MB or smaller.");
    }

    internal static bool IsAllowedExtension(string extension)
    {
        return AllowedExtensions.Contains(extension);
    }

    private static bool HasAllowedExtension(string originalFileName)
    {
        return IsAllowedExtension(Path.GetExtension(originalFileName));
    }
}
