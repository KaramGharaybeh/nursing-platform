namespace NursingPlatform.Application.PreparationPackages.ExamSessions.Exceptions;

public sealed class PackageExamSessionConflictException : InvalidOperationException
{
    public PackageExamSessionConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
