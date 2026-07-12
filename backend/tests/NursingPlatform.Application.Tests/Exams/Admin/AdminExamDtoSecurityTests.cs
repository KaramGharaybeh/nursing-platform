using System.Reflection;
using NursingPlatform.Application.Exams.Admin.DTOs;

namespace NursingPlatform.Application.Tests.Exams.Admin;

public class AdminExamDtoSecurityTests
{
    private static readonly string[] ForbiddenPropertyNames =
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
        "SelectedExamSessionAnswerOptionId"
    ];

    [Fact]
    public void AdminExamDtos_ShouldNotExposeAccountInternalsOrPaymentFields()
    {
        var dtoTypes = new[]
        {
            typeof(AdminExamCategoryDto),
            typeof(AdminExamDto),
            typeof(AdminExamVersionDto),
            typeof(AdminExamQuestionDto),
            typeof(AdminExamAnswerOptionDto),
            typeof(AdminExamVersionValidationDto)
        };

        foreach (var dtoType in dtoTypes)
        {
            var properties = FlattenPublicPropertyNames(dtoType);
            foreach (var forbidden in ForbiddenPropertyNames)
            {
                Assert.DoesNotContain(forbidden, properties, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void AdminExamQuestionDto_MayExposeIsCorrectAndExplanationForAdminOnly()
    {
        var properties = FlattenPublicPropertyNames(typeof(AdminExamQuestionDto));

        Assert.Contains("Explanation", properties);
        Assert.Contains("IsCorrect", properties);
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
                    propertyType = propertyType.IsGenericType ? propertyType.GetGenericArguments()[0] : propertyType;
                }

                if (propertyType.Assembly == type.Assembly)
                {
                    Visit(propertyType);
                }
            }
        }
    }
}
