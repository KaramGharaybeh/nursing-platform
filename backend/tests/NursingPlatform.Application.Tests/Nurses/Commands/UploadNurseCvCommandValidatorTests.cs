using FluentValidation.TestHelper;
using NursingPlatform.Application.Nurses.Commands.UploadNurseCv;

namespace NursingPlatform.Application.Tests.Nurses.Commands;

public class UploadNurseCvCommandValidatorTests
{
    private readonly UploadNurseCvCommandValidator _validator = new();

    [Fact]
    public void Upload_UnsupportedContentType_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            ContentType = "text/plain"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ContentType);
    }

    [Fact]
    public void Upload_UnsupportedExtension_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            OriginalFileName = "cv.txt"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OriginalFileName);
    }

    [Fact]
    public void Upload_FileGreaterThanFiveMegabytes_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            FileSizeBytes = (5 * 1024 * 1024) + 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileSizeBytes);
    }

    [Fact]
    public void Upload_EmptyFile_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            File = new MemoryStream(),
            FileSizeBytes = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileSizeBytes);
    }

    private static UploadNurseCvCommand CreateValidCommand()
    {
        return new UploadNurseCvCommand
        {
            File = new MemoryStream("test cv content"u8.ToArray()),
            OriginalFileName = "cv.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 14
        };
    }
}
