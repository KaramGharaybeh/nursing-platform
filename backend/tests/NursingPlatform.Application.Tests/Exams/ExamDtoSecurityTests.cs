using System.Reflection;
using NursingPlatform.Application.Exams.DTOs;

namespace NursingPlatform.Application.Tests.Exams;

public class ExamDtoSecurityTests
{
    private static readonly string[] GlobalForbiddenProperties =
    [
        "UserId",
        "Email",
        "PasswordHash",
        "Roles",
        "Permissions",
        "AccessToken",
        "RefreshToken",
        "TokenHash",
        "PaymentProviderId",
        "PaymentIntentId",
        "OrderId",
        "User",
        "NurseProfile",
        "ExamVersion"
    ];

    private static readonly string[] InProgressForbiddenProperties =
    [
        "IsCorrect",
        "CorrectAnswerOptionId",
        "Explanation",
        "Score",
        "Percentage",
        "Passed"
    ];

    [Fact]
    public void ExamSessionDto_ShouldNotExposeCorrectAnswersExplanationsOrScoringBeforeCompletion()
    {
        var properties = FlattenPublicPropertyNames(typeof(ExamSessionDto));

        foreach (var property in InProgressForbiddenProperties)
        {
            Assert.DoesNotContain(property, properties, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ExamSessionDto_MayExposeOwnSelectedAnswerOptionIdForInProgressResume()
    {
        var properties = FlattenPublicPropertyNames(typeof(ExamSessionDto));

        Assert.Contains("SelectedExamSessionAnswerOptionId", properties);
    }

    [Fact]
    public void ExamSessionReviewDto_ShouldNotExposeAccountInternals()
    {
        var properties = FlattenPublicPropertyNames(typeof(ExamSessionReviewDto));

        foreach (var property in GlobalForbiddenProperties)
        {
            Assert.DoesNotContain(property, properties, StringComparer.OrdinalIgnoreCase);
        }
    }

    private static HashSet<string> FlattenPublicPropertyNames(Type type)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<Type>();

        Visit(type);
        return names;

        void Visit(Type current)
        {
            if (!visited.Add(current) || current == typeof(string))
            {
                return;
            }

            foreach (var property in current.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                names.Add(property.Name);
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (propertyType != typeof(string) && propertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                {
                    propertyType = propertyType.IsGenericType
                        ? propertyType.GetGenericArguments()[0]
                        : propertyType;
                }

                if (propertyType.Assembly == type.Assembly)
                {
                    Visit(propertyType);
                }
            }
        }
    }
}
