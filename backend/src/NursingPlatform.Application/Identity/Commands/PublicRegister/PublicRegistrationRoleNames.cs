namespace NursingPlatform.Application.Identity.Commands.PublicRegister;

public static class PublicRegistrationRoleNames
{
    public const string Nurse = "Nurse";

    public static bool IsAllowed(string roleName)
    {
        return roleName == Nurse;
    }
}
