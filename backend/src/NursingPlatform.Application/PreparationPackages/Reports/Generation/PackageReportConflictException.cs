namespace NursingPlatform.Application.PreparationPackages.Reports.Generation;

public sealed class PackageReportConflictException : InvalidOperationException
{
    public PackageReportConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
