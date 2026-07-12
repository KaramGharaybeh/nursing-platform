using FluentValidation;

namespace NursingPlatform.Application.Payments.Common;

internal static class PaymentMoneyValidator
{
    public static IRuleBuilderOptions<T, string> ValidCurrency<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");
    }

    public static string NormalizeCurrency(string currency) => currency.Trim().ToUpperInvariant();
}
