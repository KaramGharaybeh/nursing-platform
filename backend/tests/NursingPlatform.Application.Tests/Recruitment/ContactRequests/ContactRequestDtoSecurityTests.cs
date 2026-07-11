using NursingPlatform.Application.Recruitment.DTOs;

namespace NursingPlatform.Application.Tests.Recruitment.ContactRequests;

public class ContactRequestDtoSecurityTests
{
    private static readonly string[] ForbiddenPropertyNames =
    [
        "UserId",
        "EmployerProfileId",
        "EmployerOrganizationId",
        "Email",
        "Phone",
        "PasswordHash",
        "Roles",
        "Permissions",
        "AccessToken",
        "RefreshToken",
        "TokenHash",
        "CvStorageKey",
        "CvFileUrl",
        "FileUrl",
        "InternalPath",
        "LicenseNumber",
        "User",
        "NurseProfile",
        "EmployerProfile",
        "EmployerOrganization",
        "Message",
        "RejectionReason",
        "RowVersion",
        "ConcurrencyToken"
    ];

    [Fact]
    public void ContactRequestDto_ShouldNotExposeInternalOrSensitiveFields()
    {
        AssertForbiddenPropertiesAreAbsent(typeof(ContactRequestDto));
    }

    [Fact]
    public void ReceivedContactRequestDto_ShouldNotExposeInternalOrSensitiveFields()
    {
        AssertForbiddenPropertiesAreAbsent(typeof(ReceivedContactRequestDto));
    }

    private static void AssertForbiddenPropertiesAreAbsent(Type dtoType)
    {
        var publicProperties = dtoType.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var forbidden in ForbiddenPropertyNames)
        {
            Assert.DoesNotContain(forbidden, publicProperties);
        }
    }
}
