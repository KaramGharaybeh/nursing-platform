namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public static class PublicRegistrationRoleNames
{
    public const string Nurse = "Nurse";
    public const string Employer = "Employer";

    public static bool IsAllowed(string roleName)
    {
        return roleName is Nurse or Employer;
    }
}
