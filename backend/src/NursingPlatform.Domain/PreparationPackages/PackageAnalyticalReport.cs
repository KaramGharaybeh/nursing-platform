using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Domain.PreparationPackages;

public class PackageAnalyticalReport : AuditableEntity
{
    private readonly List<PackageAnalyticalReportTopicResult> _topicResults = [];
    private readonly List<PackageAnalyticalReportGuidanceItem> _guidanceItems = [];

    private PackageAnalyticalReport()
    {
    }

    public Guid Id { get; private set; }
    public Guid NurseProfileId { get; private set; }
    public Guid ExamSessionId { get; private set; }
    public Guid ExamSessionProvenanceId { get; private set; }
    public Guid PackagePurchaseEntitlementId { get; private set; }
    public Guid PackageOrderItemSnapshotId { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public Guid PaymentOrderItemId { get; private set; }
    public Guid PreparationPackageDefinitionId { get; private set; }
    public Guid PreparationPackageVersionId { get; private set; }
    public Guid PreparationPackageOfferId { get; private set; }
    public Guid IncludedExamId { get; private set; }
    public Guid IncludedExamVersionId { get; private set; }
    public Guid ReportingProfilePublicationId { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public ExamSessionStatus FinalizedSessionStatus { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime FinalizedAt { get; private set; }
    public int Score { get; private set; }
    public int MaxScore { get; private set; }
    public decimal Percentage { get; private set; }
    public bool Passed { get; private set; }
    public int CorrectCount { get; private set; }
    public int QuestionCount { get; private set; }
    public DateTime PackageAccessStartsAt { get; private set; }
    public DateTime PackageAccessEndsAt { get; private set; }
    public IReadOnlyCollection<PackageAnalyticalReportTopicResult> TopicResults => _topicResults.AsReadOnly();
    public IReadOnlyCollection<PackageAnalyticalReportGuidanceItem> GuidanceItems => _guidanceItems.AsReadOnly();

    public static PackageAnalyticalReport Create(
        Guid nurseProfileId,
        Guid examSessionId,
        Guid examSessionProvenanceId,
        Guid packagePurchaseEntitlementId,
        Guid packageOrderItemSnapshotId,
        Guid paymentOrderId,
        Guid paymentOrderItemId,
        Guid preparationPackageDefinitionId,
        Guid preparationPackageVersionId,
        Guid preparationPackageOfferId,
        Guid includedExamId,
        Guid includedExamVersionId,
        Guid reportingProfilePublicationId,
        Guid practiceCollectionVersionId,
        DateTime generatedAt,
        ExamSessionStatus finalizedSessionStatus,
        DateTime? submittedAt,
        DateTime finalizedAt,
        int score,
        int maxScore,
        decimal percentage,
        bool passed,
        int correctCount,
        int questionCount,
        DateTime packageAccessStartsAt,
        DateTime packageAccessEndsAt)
    {
        RequireNotEmpty(nurseProfileId, nameof(nurseProfileId));
        RequireNotEmpty(examSessionId, nameof(examSessionId));
        RequireNotEmpty(examSessionProvenanceId, nameof(examSessionProvenanceId));
        RequireNotEmpty(packagePurchaseEntitlementId, nameof(packagePurchaseEntitlementId));
        RequireNotEmpty(packageOrderItemSnapshotId, nameof(packageOrderItemSnapshotId));
        RequireNotEmpty(paymentOrderId, nameof(paymentOrderId));
        RequireNotEmpty(paymentOrderItemId, nameof(paymentOrderItemId));
        RequireNotEmpty(preparationPackageDefinitionId, nameof(preparationPackageDefinitionId));
        RequireNotEmpty(preparationPackageVersionId, nameof(preparationPackageVersionId));
        RequireNotEmpty(preparationPackageOfferId, nameof(preparationPackageOfferId));
        RequireNotEmpty(includedExamId, nameof(includedExamId));
        RequireNotEmpty(includedExamVersionId, nameof(includedExamVersionId));
        RequireNotEmpty(reportingProfilePublicationId, nameof(reportingProfilePublicationId));
        RequireNotEmpty(practiceCollectionVersionId, nameof(practiceCollectionVersionId));
        RequireUtc(generatedAt, nameof(generatedAt));
        RequireUtc(finalizedAt, nameof(finalizedAt));
        RequireUtc(packageAccessStartsAt, nameof(packageAccessStartsAt));
        RequireUtc(packageAccessEndsAt, nameof(packageAccessEndsAt));

        if (submittedAt.HasValue)
        {
            RequireUtc(submittedAt.Value, nameof(submittedAt));
        }

        if (finalizedSessionStatus is not ExamSessionStatus.Submitted and not ExamSessionStatus.Expired)
        {
            throw new InvalidOperationException("Package analytical report requires a submitted or expired finalized session.");
        }

        if (packageAccessEndsAt <= packageAccessStartsAt)
        {
            throw new InvalidOperationException("Package analytical report access end must be after access start.");
        }

        if (score < 0 || maxScore < 0 || score > maxScore)
        {
            throw new InvalidOperationException("Report score must be non-negative and cannot exceed max score.");
        }

        if (percentage < 0m || percentage > 100m)
        {
            throw new InvalidOperationException("Report percentage must be between zero and one hundred.");
        }

        if (correctCount < 0 || questionCount < 0 || correctCount > questionCount)
        {
            throw new InvalidOperationException("Report correct count must be between zero and question count.");
        }

        return new PackageAnalyticalReport
        {
            Id = Guid.NewGuid(),
            NurseProfileId = nurseProfileId,
            ExamSessionId = examSessionId,
            ExamSessionProvenanceId = examSessionProvenanceId,
            PackagePurchaseEntitlementId = packagePurchaseEntitlementId,
            PackageOrderItemSnapshotId = packageOrderItemSnapshotId,
            PaymentOrderId = paymentOrderId,
            PaymentOrderItemId = paymentOrderItemId,
            PreparationPackageDefinitionId = preparationPackageDefinitionId,
            PreparationPackageVersionId = preparationPackageVersionId,
            PreparationPackageOfferId = preparationPackageOfferId,
            IncludedExamId = includedExamId,
            IncludedExamVersionId = includedExamVersionId,
            ReportingProfilePublicationId = reportingProfilePublicationId,
            PracticeCollectionVersionId = practiceCollectionVersionId,
            GeneratedAt = generatedAt,
            FinalizedSessionStatus = finalizedSessionStatus,
            SubmittedAt = submittedAt,
            FinalizedAt = finalizedAt,
            Score = score,
            MaxScore = maxScore,
            Percentage = percentage,
            Passed = passed,
            CorrectCount = correctCount,
            QuestionCount = questionCount,
            PackageAccessStartsAt = packageAccessStartsAt,
            PackageAccessEndsAt = packageAccessEndsAt,
            CreatedAt = generatedAt,
            UpdatedAt = generatedAt
        };
    }

    public void AddTopicResult(
        Guid reportingTopicId,
        string topicNameSnapshot,
        string? topicDescriptionSnapshot,
        int scoredQuestionCount,
        int correctCount,
        int earnedPoints,
        int availablePoints,
        decimal percentage,
        int sortOrder)
    {
        if (_topicResults.Any(topic => topic.ReportingTopicId == reportingTopicId))
        {
            throw new InvalidOperationException("Package analytical report topic result already exists for this topic.");
        }

        if (_topicResults.Any(topic => topic.SortOrder == sortOrder))
        {
            throw new InvalidOperationException("Package analytical report topic result sort order must be unique.");
        }

        _topicResults.Add(PackageAnalyticalReportTopicResult.Create(
            Id,
            reportingTopicId,
            topicNameSnapshot,
            topicDescriptionSnapshot,
            scoredQuestionCount,
            correctCount,
            earnedPoints,
            availablePoints,
            percentage,
            sortOrder));
    }

    public void AddGuidanceItem(
        Guid reportingTopicId,
        PackageReportGuidanceSourceType sourceType,
        Guid sourceVersionId,
        string titleSnapshot,
        string? sourceMetadataSnapshot,
        int sortOrder)
    {
        if (_guidanceItems.Any(item => item.SortOrder == sortOrder))
        {
            throw new InvalidOperationException("Package analytical report guidance item sort order must be unique.");
        }

        _guidanceItems.Add(PackageAnalyticalReportGuidanceItem.Create(
            Id,
            reportingTopicId,
            sourceType,
            sourceVersionId,
            titleSnapshot,
            sourceMetadataSnapshot,
            sortOrder));
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }

    private static void RequireUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{name} must be UTC.");
        }
    }
}
