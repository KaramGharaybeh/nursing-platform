using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Authorization;

public sealed class PackageBenefitAuthorizationResult
{
    public bool IsAuthorized { get; init; }
    public bool IsVisibleDormantRight { get; init; }
    public Guid? PackagePurchaseEntitlementId { get; init; }
    public PackageBenefitRightType RequestedRightType { get; init; }
    public string EntitlementStatus { get; init; } = string.Empty;
    public string RightStatus { get; init; } = string.Empty;
    public string? DenialReason { get; init; }
}
