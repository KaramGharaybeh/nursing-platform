using FluentValidation;

namespace NursingPlatform.Application.Identity.Queries.ListUsers;

public class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    private static readonly string[] ValidSorts =
    [
        "email",
        "-email",
        "firstName",
        "-firstName",
        "lastName",
        "-lastName",
        "createdAt",
        "-createdAt",
        "lastLoginAt",
        "-lastLoginAt"
    ];

    public ListUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.Sort)
            .Must(sort => string.IsNullOrEmpty(sort) || ValidSorts.Contains(sort))
            .WithMessage("Sort value is not valid.");
    }
}
