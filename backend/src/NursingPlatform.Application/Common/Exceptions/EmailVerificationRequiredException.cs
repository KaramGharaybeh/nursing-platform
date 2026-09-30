namespace NursingPlatform.Application.Common.Exceptions;

public sealed class EmailVerificationRequiredException : InvalidOperationException
{
    public const string ErrorCode = "email_verification_required";

    public EmailVerificationRequiredException()
        : base("Email verification is required before signing in.")
    {
    }

    public string Code => ErrorCode;
}
