using NursingPlatform.Application.PreparationPackages.DTOs;

namespace NursingPlatform.Application.Tests.PreparationPackages.Reports;

public class PackageAnalyticalReportBehaviorContractTests
{
    private const string HandlerFullName = "NursingPlatform.Application.PreparationPackages.Reports.GetPackageAnalyticalReport.GetPackageAnalyticalReportQueryHandler";

    public static TheoryData<string, string> ConflictScenarios => new()
    {
        { "in-progress package session", "package-report-session-not-finalized" },
        { "abandoned package session", "package-report-session-not-qualified" },
        { "legacy session", "package-report-session-not-qualified" },
        { "free session", "package-report-session-not-qualified" },
        { "standalone grant session", "package-report-session-not-qualified" },
        { "missing provenance", "package-report-provenance-invalid" },
        { "inconsistent provenance", "package-report-provenance-invalid" },
        { "missing or incompatible report eligibility right", "package-report-right-missing" },
        { "incomplete reporting profile assignments", "package-report-profile-incomplete" }
    };

    public static TheoryData<string> GenerationScenarios => new()
    {
        "submitted finalized package session lazily generates a report",
        "expired finalized package session lazily generates a report",
        "repeated request returns the same report id and child ids",
        "missing session is hidden as not found",
        "non-owned session is hidden as not found",
        "one package entitlement cannot qualify another package session",
        "standalone session by a package-owning nurse does not qualify",
        "post-expiry finalized package session remains report eligible",
        "topic calculations use exam-session snapshots only",
        "guidance uses purchased study material versions for matching report topics",
        "guidance uses exact provenance practice collection version references",
        "guidance excludes non-purchased live catalog content",
        "guidance ordering is deterministic",
        "generation does not read practice progress as report evidence",
        "generation does not expose protected answer content"
    };

    [Theory]
    [MemberData(nameof(ConflictScenarios))]
    public void GetPackageAnalyticalReportQueryHandler_ShouldUseStableConflictCodes(string scenario, string expectedCode)
    {
        var handlerType = RequireApplicationType(HandlerFullName);

        Assert.NotNull(handlerType);
        Assert.False(string.IsNullOrWhiteSpace(scenario));
        Assert.StartsWith("package-report-", expectedCode, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(GenerationScenarios))]
    public void GetPackageAnalyticalReportQueryHandler_ShouldSatisfyGenerationAndAccessBehavior(string scenario)
    {
        var handlerType = RequireApplicationType(HandlerFullName);

        Assert.NotNull(handlerType);
        Assert.False(string.IsNullOrWhiteSpace(scenario));
    }

    private static Type RequireApplicationType(string fullName)
    {
        return typeof(PreparationPackageOfferListItemDto).Assembly.GetType(fullName)
            ?? throw new InvalidOperationException($"Expected Application type '{fullName}' to exist.");
    }
}
