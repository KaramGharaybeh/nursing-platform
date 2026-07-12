using System.Reflection;
using NursingPlatform.Application.Exams.Analytics.DTOs;

namespace NursingPlatform.Application.Tests.Exams.Analytics;

public class ExamAnalyticsDtoSecurityTests
{
    private static readonly string[] ForbiddenProperties =
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
        "ExamSession",
        "ExamSessionAnswer",
        "ExamSessionQuestion",
        "ExamSessionAnswerOption",
        "SelectedExamSessionAnswerOptionId",
        "CorrectAnswerOptionId",
        "IsCorrect",
        "Explanation"
    ];

    [Fact]
    public void ExamAnalyticsDtos_ShouldNotExposeAccountInternalsPaymentFieldsOrAnswerKeys()
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in AnalyticsDtoTypes())
        {
            properties.UnionWith(FlattenPublicPropertyNames(type));
        }

        foreach (var property in ForbiddenProperties)
        {
            Assert.DoesNotContain(property, properties);
        }
    }

    [Fact]
    public void ExamAnalyticsDtos_ShouldExposeAggregateMetricsOnly()
    {
        var summaryProperties = FlattenPublicPropertyNames(typeof(ExamAnalyticsSummaryDto));
        var trendProperties = FlattenPublicPropertyNames(typeof(ExamAnalyticsTrendPointDto));

        Assert.Contains("InProgressCount", summaryProperties);
        Assert.Contains("AttemptCount", summaryProperties);
        Assert.Contains("CountedAttemptCount", summaryProperties);
        Assert.Contains("AttemptCount", trendProperties);
        Assert.Contains("CountedAttemptCount", trendProperties);
    }

    private static Type[] AnalyticsDtoTypes() =>
    [
        typeof(ExamAnalyticsSummaryDto),
        typeof(ExamAnalyticsByExamDto),
        typeof(ExamAnalyticsByCategoryDto),
        typeof(ExamAnalyticsTrendPointDto)
    ];

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
