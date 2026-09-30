namespace NursingPlatform.Domain.PreparationPackages;

public class PackageAnalyticalReportTopicResult
{
    private PackageAnalyticalReportTopicResult()
    {
    }

    public Guid Id { get; private set; }
    public Guid PackageAnalyticalReportId { get; private set; }
    public Guid ReportingTopicId { get; private set; }
    public string TopicNameSnapshot { get; private set; } = string.Empty;
    public string? TopicDescriptionSnapshot { get; private set; }
    public int ScoredQuestionCount { get; private set; }
    public int CorrectCount { get; private set; }
    public int EarnedPoints { get; private set; }
    public int AvailablePoints { get; private set; }
    public decimal Percentage { get; private set; }
    public int SortOrder { get; private set; }

    internal static PackageAnalyticalReportTopicResult Create(
        Guid packageAnalyticalReportId,
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
        RequireNotEmpty(packageAnalyticalReportId, nameof(packageAnalyticalReportId));
        RequireNotEmpty(reportingTopicId, nameof(reportingTopicId));

        if (scoredQuestionCount < 0)
        {
            throw new InvalidOperationException("Topic scored question count cannot be negative.");
        }

        if (correctCount < 0 || correctCount > scoredQuestionCount)
        {
            throw new InvalidOperationException("Topic correct count must be between zero and scored question count.");
        }

        if (earnedPoints < 0 || availablePoints < 0 || earnedPoints > availablePoints)
        {
            throw new InvalidOperationException("Topic points must be non-negative and earned points cannot exceed available points.");
        }

        if (percentage < 0m || percentage > 100m)
        {
            throw new InvalidOperationException("Topic percentage must be between zero and one hundred.");
        }

        if (sortOrder < 1)
        {
            throw new InvalidOperationException("Topic result sort order must be positive.");
        }

        return new PackageAnalyticalReportTopicResult
        {
            Id = Guid.NewGuid(),
            PackageAnalyticalReportId = packageAnalyticalReportId,
            ReportingTopicId = reportingTopicId,
            TopicNameSnapshot = RequireText(topicNameSnapshot, nameof(topicNameSnapshot)),
            TopicDescriptionSnapshot = NormalizeOptional(topicDescriptionSnapshot),
            ScoredQuestionCount = scoredQuestionCount,
            CorrectCount = correctCount,
            EarnedPoints = earnedPoints,
            AvailablePoints = availablePoints,
            Percentage = percentage,
            SortOrder = sortOrder
        };
    }

    private static void RequireNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{name} is required.");
        }
    }

    private static string RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
