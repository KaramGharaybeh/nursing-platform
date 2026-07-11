using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Common.Exceptions;
using NursingPlatform.Application.Employers.Common;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;
using NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;
using NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;
using NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.Recruitment;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Recruitment.ContactRequests;

public class ContactRequestHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_Create_WithEligibleEmployerAndCandidate_CreatesPendingRequestWithSnapshots()
    {
        var employerUserId = Guid.NewGuid();
        var employerProfile = CreateEmployerProfile(employerUserId);
        var organization = CreateOrganization(employerProfile.Id);
        var candidate = CreateEligibleNurseProfile("Critical care nurse");
        var contactRequests = new List<ContactRequest>();
        var handler = CreateCreateHandler(employerUserId, [CreateUserWithRole(employerUserId, "Employer")], [employerProfile], [organization], [candidate], contactRequests);

        var result = await handler.Handle(new CreateContactRequestCommand { NurseProfileId = candidate.Id }, CancellationToken.None);

        Assert.Equal(candidate.Id, result.NurseProfileId);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("Critical care nurse", result.CandidateHeadline);
        var created = Assert.Single(contactRequests);
        Assert.Equal(employerProfile.Id, created.EmployerProfileId);
        Assert.Equal(organization.Id, created.EmployerOrganizationId);
        Assert.Equal("General Hospital", created.EmployerOrganizationNameSnapshot);
        Assert.Equal("Recruitment Manager", created.JobTitleSnapshot);
        Assert.Equal("Clinical Hiring", created.DepartmentSnapshot);
        Assert.Null(created.RespondedAt);
        Assert.Null(created.CancelledAt);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Create_WhenEmployerProfileMissing_ThrowsForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var handler = CreateCreateHandler(userId, [CreateUserWithRole(userId, "Employer")], [], [], [], []);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateContactRequestCommand { NurseProfileId = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Create_WhenTargetNurseProfileMissingOrIneligible_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var handler = CreateCreateHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [CreateOrganization(profile.Id)], [], []);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CreateContactRequestCommand { NurseProfileId = Guid.NewGuid() }, CancellationToken.None));
    }

    [Theory]
    [InlineData(ContactRequestStatus.Pending)]
    [InlineData(ContactRequestStatus.Approved)]
    public async Task Handle_Create_WithActiveDuplicate_ThrowsInvalidOperationException(ContactRequestStatus status)
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var organization = CreateOrganization(profile.Id);
        var candidate = CreateEligibleNurseProfile("ICU nurse");
        var existing = CreateContactRequest(profile.Id, organization.Id, candidate.Id, status, DateTime.UtcNow);
        var handler = CreateCreateHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [organization], [candidate], [existing]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateContactRequestCommand { NurseProfileId = candidate.Id }, CancellationToken.None));
    }

    [Theory]
    [InlineData(ContactRequestStatus.Rejected)]
    [InlineData(ContactRequestStatus.Cancelled)]
    public async Task Handle_Create_WithTerminalHistory_CreatesNewPendingRequest(ContactRequestStatus status)
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var organization = CreateOrganization(profile.Id);
        var candidate = CreateEligibleNurseProfile("ICU nurse");
        var requests = new List<ContactRequest> { CreateContactRequest(profile.Id, organization.Id, candidate.Id, status, DateTime.UtcNow) };
        var handler = CreateCreateHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [organization], [candidate], requests);

        await handler.Handle(new CreateContactRequestCommand { NurseProfileId = candidate.Id }, CancellationToken.None);

        Assert.Equal(2, requests.Count);
        Assert.Contains(requests, r => r.Status == ContactRequestStatus.Pending);
    }

    [Fact]
    public async Task Handle_ListMy_ReturnsOnlyCurrentEmployerRequestsWithPaginationAndStatusFilter()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var organizationId = Guid.NewGuid();
        var otherEmployerProfileId = Guid.NewGuid();
        var requests = Enumerable.Range(1, 12)
            .Select(i => CreateContactRequest(profile.Id, organizationId, Guid.NewGuid(), ContactRequestStatus.Pending, new DateTime(2026, 7, i, 0, 0, 0, DateTimeKind.Utc)))
            .Append(CreateContactRequest(otherEmployerProfileId, organizationId, Guid.NewGuid(), ContactRequestStatus.Pending, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)))
            .Append(CreateContactRequest(profile.Id, organizationId, Guid.NewGuid(), ContactRequestStatus.Rejected, new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc)))
            .ToList();
        var handler = CreateListMyHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], requests);

        var result = await handler.Handle(new ListMyContactRequestsQuery { Page = 2, PageSize = 5, Status = ContactRequestStatus.Pending }, CancellationToken.None);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(requests.Where(r => r.EmployerProfileId == profile.Id && r.Status == ContactRequestStatus.Pending).OrderByDescending(r => r.CreatedAt).ThenBy(r => r.Id).Skip(5).First().Id, result.Items[0].Id);
        Assert.All(result.Items, item => Assert.Equal("Pending", item.Status));
    }

    [Fact]
    public async Task Handle_GetMy_WhenNotOwned_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var request = CreateContactRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ContactRequestStatus.Pending, DateTime.UtcNow);
        var handler = CreateGetMyHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [request]);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetMyContactRequestQuery { Id = request.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Cancel_WhenOwnedPending_AtomicallyCancelsAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var request = CreateContactRequest(profile.Id, Guid.NewGuid(), Guid.NewGuid(), ContactRequestStatus.Pending, DateTime.UtcNow);
        SetupTransitionMutation(request, profile.Id, isEmployerOwner: true, ContactRequestStatus.Cancelled, 1);
        var handler = CreateCancelHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [request]);

        var result = await handler.Handle(new CancelContactRequestCommand { Id = request.Id }, CancellationToken.None);

        Assert.Equal("Cancelled", result.Status);
        Assert.NotNull(result.CancelledAt);
        _contextMock.Verify(c => c.ExecuteContactRequestTransitionAsync(
            request.Id,
            profile.Id,
            true,
            ContactRequestStatus.Cancelled,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Cancel_WhenTerminal_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var request = CreateContactRequest(profile.Id, Guid.NewGuid(), Guid.NewGuid(), ContactRequestStatus.Approved, DateTime.UtcNow);
        SetupTransitionMutation(request, profile.Id, isEmployerOwner: true, ContactRequestStatus.Cancelled, 0);
        var handler = CreateCancelHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [request]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CancelContactRequestCommand { Id = request.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ListReceived_ReturnsOnlyCurrentNurseRequestsWithPaginationAndStatusFilter()
    {
        var userId = Guid.NewGuid();
        var nurseProfile = CreateEligibleNurseProfile("ICU nurse");
        nurseProfile.UserId = userId;
        var requests = Enumerable.Range(1, 8)
            .Select(i => CreateContactRequest(Guid.NewGuid(), Guid.NewGuid(), nurseProfile.Id, ContactRequestStatus.Pending, new DateTime(2026, 7, i, 0, 0, 0, DateTimeKind.Utc)))
            .Append(CreateContactRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ContactRequestStatus.Pending, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)))
            .Append(CreateContactRequest(Guid.NewGuid(), Guid.NewGuid(), nurseProfile.Id, ContactRequestStatus.Approved, new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc)))
            .ToList();
        var handler = CreateListReceivedHandler(userId, [CreateUserWithRole(userId, "Nurse")], [nurseProfile], requests);

        var result = await handler.Handle(new ListReceivedContactRequestsQuery { Page = 2, PageSize = 3, Status = ContactRequestStatus.Pending }, CancellationToken.None);

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, item =>
        {
            Assert.Equal("Pending", item.Status);
            Assert.Equal("General Hospital", item.OrganizationName);
        });
    }

    [Theory]
    [InlineData(ContactRequestStatus.Approved, "Approved")]
    [InlineData(ContactRequestStatus.Rejected, "Rejected")]
    public async Task Handle_ReceivedTransition_WhenOwnedPending_AtomicallyTransitionsAndReturnsDto(
        ContactRequestStatus transition,
        string expectedStatus)
    {
        var userId = Guid.NewGuid();
        var nurseProfile = CreateEligibleNurseProfile("ICU nurse");
        nurseProfile.UserId = userId;
        var request = CreateContactRequest(Guid.NewGuid(), Guid.NewGuid(), nurseProfile.Id, ContactRequestStatus.Pending, DateTime.UtcNow);
        SetupTransitionMutation(request, nurseProfile.Id, isEmployerOwner: false, transition, 1);
        SetupContext(userId, [CreateUserWithRole(userId, "Nurse")], [], [], [nurseProfile], [request]);
        var guard = new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object);

        var result = transition == ContactRequestStatus.Approved
            ? await new ApproveReceivedContactRequestCommandHandler(_contextMock.Object, guard).Handle(new ApproveReceivedContactRequestCommand { Id = request.Id }, CancellationToken.None)
            : await new RejectReceivedContactRequestCommandHandler(_contextMock.Object, guard).Handle(new RejectReceivedContactRequestCommand { Id = request.Id }, CancellationToken.None);

        Assert.Equal(expectedStatus, result.Status);
        Assert.NotNull(result.RespondedAt);
        Assert.Equal("General Hospital", result.OrganizationName);
    }

    [Fact]
    public async Task Handlers_DoNotExposeEmailPhoneUserIdOrInternalFksInDtos()
    {
        var userId = Guid.NewGuid();
        var profile = CreateEmployerProfile(userId);
        var request = CreateContactRequest(profile.Id, Guid.NewGuid(), Guid.NewGuid(), ContactRequestStatus.Pending, DateTime.UtcNow);
        var handler = CreateGetMyHandler(userId, [CreateUserWithRole(userId, "Employer")], [profile], [request]);

        var result = await handler.Handle(new GetMyContactRequestQuery { Id = request.Id }, CancellationToken.None);

        Assert.DoesNotContain(result.GetType().GetProperties().Select(p => p.Name), p =>
            p is "Email" or "Phone" or "UserId" or "EmployerProfileId" or "EmployerOrganizationId");
    }

    private CreateContactRequestCommandHandler CreateCreateHandler(
        Guid currentUserId,
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<EmployerProfile> employerProfiles,
        IReadOnlyCollection<EmployerOrganization> organizations,
        IReadOnlyCollection<NurseProfile> nurseProfiles,
        List<ContactRequest> contactRequests)
    {
        SetupContext(currentUserId, users, employerProfiles, organizations, nurseProfiles, contactRequests);
        return new CreateContactRequestCommandHandler(_contextMock.Object, new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private ListMyContactRequestsQueryHandler CreateListMyHandler(Guid currentUserId, IReadOnlyCollection<User> users, IReadOnlyCollection<EmployerProfile> profiles, List<ContactRequest> requests)
    {
        SetupContext(currentUserId, users, profiles, [], [], requests);
        return new ListMyContactRequestsQueryHandler(_contextMock.Object, new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private GetMyContactRequestQueryHandler CreateGetMyHandler(Guid currentUserId, IReadOnlyCollection<User> users, IReadOnlyCollection<EmployerProfile> profiles, List<ContactRequest> requests)
    {
        SetupContext(currentUserId, users, profiles, [], [], requests);
        return new GetMyContactRequestQueryHandler(_contextMock.Object, new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private CancelContactRequestCommandHandler CreateCancelHandler(Guid currentUserId, IReadOnlyCollection<User> users, IReadOnlyCollection<EmployerProfile> profiles, List<ContactRequest> requests)
    {
        SetupContext(currentUserId, users, profiles, [], [], requests);
        return new CancelContactRequestCommandHandler(_contextMock.Object, new EmployerRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private ListReceivedContactRequestsQueryHandler CreateListReceivedHandler(Guid currentUserId, IReadOnlyCollection<User> users, IReadOnlyCollection<NurseProfile> profiles, List<ContactRequest> requests)
    {
        SetupContext(currentUserId, users, [], [], profiles, requests);
        return new ListReceivedContactRequestsQueryHandler(_contextMock.Object, new NurseRoleGuard(_contextMock.Object, _currentUserMock.Object));
    }

    private void SetupContext(
        Guid currentUserId,
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<EmployerProfile> employerProfiles,
        IReadOnlyCollection<EmployerOrganization> organizations,
        IReadOnlyCollection<NurseProfile> nurseProfiles,
        List<ContactRequest> contactRequests)
    {
        _currentUserMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _contextMock.Setup(c => c.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerProfiles).Returns(employerProfiles.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.EmployerOrganizations).Returns(organizations.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.NurseProfiles).Returns(nurseProfiles.AsQueryable().BuildMockDbSet().Object);
        var contactRequestsDbSet = contactRequests.AsQueryable().BuildMockDbSet();
        contactRequestsDbSet.Setup(s => s.Add(It.IsAny<ContactRequest>()))
            .Callback<ContactRequest>(contactRequests.Add);
        _contextMock.Setup(c => c.ContactRequests).Returns(contactRequestsDbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupTransitionMutation(ContactRequest request, Guid ownerProfileId, bool isEmployerOwner, ContactRequestStatus status, int affectedRows)
    {
        _contextMock
            .Setup(c => c.ExecuteContactRequestTransitionAsync(
                request.Id,
                ownerProfileId,
                isEmployerOwner,
                status,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, bool, ContactRequestStatus, DateTime, CancellationToken>((_, _, _, nextStatus, timestamp, _) =>
            {
                if (affectedRows == 0)
                {
                    return;
                }

                request.Status = nextStatus;
                request.UpdatedAt = timestamp;
                if (nextStatus is ContactRequestStatus.Approved or ContactRequestStatus.Rejected)
                {
                    request.RespondedAt = timestamp;
                }
                else if (nextStatus == ContactRequestStatus.Cancelled)
                {
                    request.CancelledAt = timestamp;
                }
            })
            .ReturnsAsync(affectedRows);
    }

    private static EmployerProfile CreateEmployerProfile(Guid userId)
    {
        return new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobTitle = "Recruitment Manager",
            Department = "Clinical Hiring"
        };
    }

    private static EmployerOrganization CreateOrganization(Guid employerProfileId)
    {
        return new EmployerOrganization
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = employerProfileId,
            Name = "General Hospital"
        };
    }

    private static NurseProfile CreateEligibleNurseProfile(string headline)
    {
        var userId = Guid.NewGuid();
        return new NurseProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Headline = headline,
            IsAvailableForRecruitment = true,
            LicenseCountry = new Country { Id = Guid.NewGuid(), Name = "Canada", Code = "CA", IsActive = true },
            CurrentCountry = new Country { Id = Guid.NewGuid(), Name = "United Kingdom", Code = "GB", IsActive = true },
            User = new User { Id = userId, IsActive = true, EmailVerified = true }
        };
    }

    private static ContactRequest CreateContactRequest(Guid employerProfileId, Guid employerOrganizationId, Guid nurseProfileId, ContactRequestStatus status, DateTime createdAt)
    {
        return new ContactRequest
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = employerProfileId,
            EmployerOrganizationId = employerOrganizationId,
            NurseProfileId = nurseProfileId,
            Status = status,
            CandidateHeadlineSnapshot = "ICU nurse",
            CandidateLicenseCountryNameSnapshot = "Canada",
            CandidateCurrentCountryNameSnapshot = "United Kingdom",
            EmployerOrganizationNameSnapshot = "General Hospital",
            JobTitleSnapshot = "Recruitment Manager",
            DepartmentSnapshot = "Clinical Hiring",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static User CreateUserWithRole(Guid userId, string roleName)
    {
        var roleId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            IsActive = true,
            UserRoles =
            [
                new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Role = new Role { Id = roleId, Name = roleName }
                }
            ]
        };
    }
}
