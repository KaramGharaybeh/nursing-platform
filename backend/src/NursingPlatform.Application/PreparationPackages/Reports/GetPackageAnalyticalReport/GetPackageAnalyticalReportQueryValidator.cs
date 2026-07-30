using FluentValidation;

namespace NursingPlatform.Application.PreparationPackages.Reports.GetPackageAnalyticalReport;

public sealed class GetPackageAnalyticalReportQueryValidator : AbstractValidator<GetPackageAnalyticalReportQuery>
{
    public GetPackageAnalyticalReportQueryValidator()
    {
        RuleFor(query => query.SessionId).NotEmpty();
    }
}
