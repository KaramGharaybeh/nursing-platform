using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Domain.Tests.Recruitment;

public class ContactRequestTests
{
    [Fact]
    public void ContactRequest_DefaultStatus_IsPending()
    {
        var request = new ContactRequest();

        Assert.Equal(ContactRequestStatus.Pending, request.Status);
        Assert.False(request.IsTerminal);
    }

    [Fact]
    public void ContactRequest_CapturesSafeSnapshotsWithoutContactInfo()
    {
        var request = new ContactRequest
        {
            CandidateHeadlineSnapshot = "ICU nurse",
            CandidateLicenseCountryNameSnapshot = "Canada",
            CandidateCurrentCountryNameSnapshot = "United Kingdom",
            EmployerOrganizationNameSnapshot = "General Hospital",
            JobTitleSnapshot = "Recruitment Manager",
            DepartmentSnapshot = "Clinical Hiring"
        };

        Assert.Equal("ICU nurse", request.CandidateHeadlineSnapshot);
        Assert.Equal("Canada", request.CandidateLicenseCountryNameSnapshot);
        Assert.Equal("United Kingdom", request.CandidateCurrentCountryNameSnapshot);
        Assert.Equal("General Hospital", request.EmployerOrganizationNameSnapshot);
        Assert.DoesNotContain(
            typeof(ContactRequest).GetProperties().Select(p => p.Name),
            p => p.Contains("Email", StringComparison.OrdinalIgnoreCase)
                || p.Contains("Phone", StringComparison.OrdinalIgnoreCase)
                || p.Contains("Message", StringComparison.OrdinalIgnoreCase)
                || p.Contains("RejectionReason", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(ContactRequestStatus.Pending, false)]
    [InlineData(ContactRequestStatus.Approved, true)]
    [InlineData(ContactRequestStatus.Rejected, true)]
    [InlineData(ContactRequestStatus.Cancelled, true)]
    public void ContactRequest_TerminalStatusHelpers_IdentifyApprovedRejectedAndCancelled(
        ContactRequestStatus status,
        bool expectedTerminal)
    {
        var request = new ContactRequest { Status = status };

        Assert.Equal(expectedTerminal, request.IsTerminal);
    }
}
