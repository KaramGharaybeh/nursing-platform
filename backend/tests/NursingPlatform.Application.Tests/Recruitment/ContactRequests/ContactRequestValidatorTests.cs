using FluentValidation.TestHelper;
using NursingPlatform.Application.Recruitment.Commands.ApproveReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CancelContactRequest;
using NursingPlatform.Application.Recruitment.Commands.CreateContactRequest;
using NursingPlatform.Application.Recruitment.Commands.RejectReceivedContactRequest;
using NursingPlatform.Application.Recruitment.Queries.GetMyContactRequest;
using NursingPlatform.Application.Recruitment.Queries.ListMyContactRequests;
using NursingPlatform.Application.Recruitment.Queries.ListReceivedContactRequests;
using NursingPlatform.Domain.Recruitment;

namespace NursingPlatform.Application.Tests.Recruitment.ContactRequests;

public class ContactRequestValidatorTests
{
    [Fact]
    public void Validate_CreateContactRequest_WithEmptyNurseProfileId_ShouldHaveError()
    {
        var result = new CreateContactRequestCommandValidator()
            .TestValidate(new CreateContactRequestCommand { NurseProfileId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(x => x.NurseProfileId);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validate_ListMyContactRequests_WithInvalidPagination_ShouldHaveError(int page, int pageSize)
    {
        var result = new ListMyContactRequestsQueryValidator()
            .TestValidate(new ListMyContactRequestsQuery { Page = page, PageSize = pageSize });

        if (page < 1)
        {
            result.ShouldHaveValidationErrorFor(x => x.Page);
        }

        if (pageSize is < 1 or > 100)
        {
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [Fact]
    public void Validate_ListReceivedContactRequests_WithInvalidStatus_ShouldHaveError()
    {
        var result = new ListReceivedContactRequestsQueryValidator()
            .TestValidate(new ListReceivedContactRequestsQuery { Status = (ContactRequestStatus)99 });

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_TransitionCommands_WithEmptyId_ShouldHaveError()
    {
        new CancelContactRequestCommandValidator()
            .TestValidate(new CancelContactRequestCommand { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.Id);
        new ApproveReceivedContactRequestCommandValidator()
            .TestValidate(new ApproveReceivedContactRequestCommand { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.Id);
        new RejectReceivedContactRequestCommandValidator()
            .TestValidate(new RejectReceivedContactRequestCommand { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.Id);
        new GetMyContactRequestQueryValidator()
            .TestValidate(new GetMyContactRequestQuery { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.Id);
    }
}
