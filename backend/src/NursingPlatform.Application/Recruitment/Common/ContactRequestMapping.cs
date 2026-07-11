using NursingPlatform.Application.Recruitment.DTOs;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Recruitment.Common;

internal static class ContactRequestMapping
{
    public static ContactRequestDto ToEmployerDto(ContactRequest request)
    {
        return new ContactRequestDto
        {
            Id = request.Id,
            NurseProfileId = request.NurseProfileId,
            Status = ToStatusText(request.Status),
            CandidateHeadline = request.CandidateHeadlineSnapshot,
            CandidateLicenseCountryName = request.CandidateLicenseCountryNameSnapshot,
            CandidateCurrentCountryName = request.CandidateCurrentCountryNameSnapshot,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            RespondedAt = request.RespondedAt,
            CancelledAt = request.CancelledAt
        };
    }

    public static ReceivedContactRequestDto ToNurseDto(ContactRequest request)
    {
        return new ReceivedContactRequestDto
        {
            Id = request.Id,
            OrganizationName = request.EmployerOrganizationNameSnapshot,
            JobTitle = request.JobTitleSnapshot,
            Department = request.DepartmentSnapshot,
            Status = ToStatusText(request.Status),
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            RespondedAt = request.RespondedAt,
            CancelledAt = request.CancelledAt
        };
    }

    private static string ToStatusText(ContactRequestStatus status) => status.ToString();
}
