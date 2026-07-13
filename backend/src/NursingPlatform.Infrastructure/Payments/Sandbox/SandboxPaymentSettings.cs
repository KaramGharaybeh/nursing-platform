namespace NursingPlatform.Infrastructure.Payments.Sandbox;

public class SandboxPaymentSettings
{
    public const string SectionName = "Payment:Sandbox";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string SupportedCurrency { get; set; } = "USD";
}
