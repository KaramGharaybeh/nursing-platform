using System.Reflection;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Domain.Tests.PreparationPackages;

public class PackageAnalyticalReportDomainTests
{
    [Fact]
    public void Create_CapturesRequiredSessionProvenancePackageAndScoreFacts()
    {
        var facts = CreateReportFacts();

        var report = CreateReport(facts);

        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(facts.NurseProfileId, report.NurseProfileId);
        Assert.Equal(facts.ExamSessionId, report.ExamSessionId);
        Assert.Equal(facts.ExamSessionProvenanceId, report.ExamSessionProvenanceId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, report.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PackageOrderItemSnapshotId, report.PackageOrderItemSnapshotId);
        Assert.Equal(facts.PaymentOrderId, report.PaymentOrderId);
        Assert.Equal(facts.PaymentOrderItemId, report.PaymentOrderItemId);
        Assert.Equal(facts.PreparationPackageDefinitionId, report.PreparationPackageDefinitionId);
        Assert.Equal(facts.PreparationPackageVersionId, report.PreparationPackageVersionId);
        Assert.Equal(facts.PreparationPackageOfferId, report.PreparationPackageOfferId);
        Assert.Equal(facts.IncludedExamId, report.IncludedExamId);
        Assert.Equal(facts.IncludedExamVersionId, report.IncludedExamVersionId);
        Assert.Equal(facts.ReportingProfilePublicationId, report.ReportingProfilePublicationId);
        Assert.Equal(facts.PracticeCollectionVersionId, report.PracticeCollectionVersionId);
        Assert.Equal(facts.GeneratedAt, report.GeneratedAt);
        Assert.Equal(facts.FinalizedSessionStatus, report.FinalizedSessionStatus);
        Assert.Equal(facts.SubmittedAt, report.SubmittedAt);
        Assert.Equal(facts.FinalizedAt, report.FinalizedAt);
        Assert.Equal(facts.Score, report.Score);
        Assert.Equal(facts.MaxScore, report.MaxScore);
        Assert.Equal(facts.Percentage, report.Percentage);
        Assert.Equal(facts.Passed, report.Passed);
        Assert.Equal(facts.CorrectCount, report.CorrectCount);
        Assert.Equal(facts.QuestionCount, report.QuestionCount);
        Assert.Equal(facts.PackageAccessStartsAt, report.PackageAccessStartsAt);
        Assert.Equal(facts.PackageAccessEndsAt, report.PackageAccessEndsAt);
    }

    [Fact]
    public void Create_RejectsEmptyRequiredIds()
    {
        var facts = CreateReportFacts();

        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { NurseProfileId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { ExamSessionId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { ExamSessionProvenanceId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PackagePurchaseEntitlementId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PackageOrderItemSnapshotId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PaymentOrderId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PaymentOrderItemId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PreparationPackageDefinitionId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PreparationPackageVersionId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PreparationPackageOfferId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { IncludedExamId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { IncludedExamVersionId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { ReportingProfilePublicationId = Guid.Empty }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PracticeCollectionVersionId = Guid.Empty }));
    }

    [Fact]
    public void Create_RequiresUtcTimestamps()
    {
        var facts = CreateReportFacts();

        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { GeneratedAt = DateTime.SpecifyKind(facts.GeneratedAt, DateTimeKind.Local) }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { SubmittedAt = DateTime.SpecifyKind(facts.SubmittedAt!.Value, DateTimeKind.Local) }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { FinalizedAt = DateTime.SpecifyKind(facts.FinalizedAt, DateTimeKind.Local) }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PackageAccessStartsAt = DateTime.SpecifyKind(facts.PackageAccessStartsAt, DateTimeKind.Local) }));
        Assert.Throws<InvalidOperationException>(() => CreateReport(facts with { PackageAccessEndsAt = DateTime.SpecifyKind(facts.PackageAccessEndsAt, DateTimeKind.Local) }));
    }

    [Fact]
    public void AddTopicResult_StoresCountsPointsAndPercentagesWithoutLabelsOrBands()
    {
        var report = CreateReport();
        var topicId = Guid.NewGuid();

        report.AddTopicResult(
            topicId,
            "Pharmacology",
            "Medication safety",
            scoredQuestionCount: 5,
            correctCount: 4,
            earnedPoints: 8,
            availablePoints: 10,
            percentage: 80.00m,
            sortOrder: 1);

        var topic = Assert.Single(report.TopicResults);
        Assert.NotEqual(Guid.Empty, topic.Id);
        Assert.Equal(report.Id, topic.PackageAnalyticalReportId);
        Assert.Equal(topicId, topic.ReportingTopicId);
        Assert.Equal("Pharmacology", topic.TopicNameSnapshot);
        Assert.Equal("Medication safety", topic.TopicDescriptionSnapshot);
        Assert.Equal(5, topic.ScoredQuestionCount);
        Assert.Equal(4, topic.CorrectCount);
        Assert.Equal(8, topic.EarnedPoints);
        Assert.Equal(10, topic.AvailablePoints);
        Assert.Equal(80.00m, topic.Percentage);
        Assert.Equal(1, topic.SortOrder);
        Assert.DoesNotContain(topic.GetType().GetProperties(), property => property.Name.Contains("Band", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(topic.GetType().GetProperties(), property => property.Name.Contains("Label", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddGuidanceItem_SupportsStudyMaterialVersionAndPracticeCollectionVersionReferences()
    {
        var report = CreateReport();
        var topicId = Guid.NewGuid();
        var materialVersionId = Guid.NewGuid();
        var practiceCollectionVersionId = Guid.NewGuid();

        report.AddGuidanceItem(
            topicId,
            PackageReportGuidanceSourceType.StudyMaterialVersion,
            materialVersionId,
            "Medication safety guide",
            "FormattedText",
            sortOrder: 1);
        report.AddGuidanceItem(
            topicId,
            PackageReportGuidanceSourceType.PracticeCollectionVersion,
            practiceCollectionVersionId,
            "Medication safety practice",
            "PracticeCollectionVersion",
            sortOrder: 2);

        Assert.Collection(
            report.GuidanceItems.OrderBy(item => item.SortOrder),
            material =>
            {
                Assert.Equal(PackageReportGuidanceSourceType.StudyMaterialVersion, material.SourceType);
                Assert.Equal(materialVersionId, material.SourceVersionId);
                Assert.Equal("Medication safety guide", material.TitleSnapshot);
                Assert.Equal("FormattedText", material.SourceMetadataSnapshot);
            },
            practice =>
            {
                Assert.Equal(PackageReportGuidanceSourceType.PracticeCollectionVersion, practice.SourceType);
                Assert.Equal(practiceCollectionVersionId, practice.SourceVersionId);
                Assert.Equal("Medication safety practice", practice.TitleSnapshot);
                Assert.Equal("PracticeCollectionVersion", practice.SourceMetadataSnapshot);
            });
    }

    [Fact]
    public void AddTopicResult_RejectsDuplicateTopicDuplicateOrderAndInvalidCounts()
    {
        var report = CreateReport();
        var topicId = Guid.NewGuid();

        report.AddTopicResult(topicId, "Topic", null, 2, 1, 1, 2, 50m, 1);

        Assert.Throws<InvalidOperationException>(() => report.AddTopicResult(topicId, "Other", null, 1, 1, 1, 1, 100m, 2));
        Assert.Throws<InvalidOperationException>(() => report.AddTopicResult(Guid.NewGuid(), "Other", null, 1, 1, 1, 1, 100m, 1));
        Assert.Throws<InvalidOperationException>(() => report.AddTopicResult(Guid.NewGuid(), "Other", null, 1, 2, 1, 1, 100m, 3));
        Assert.Throws<InvalidOperationException>(() => report.AddTopicResult(Guid.NewGuid(), "Other", null, 1, 1, 2, 1, 100m, 3));
        Assert.Throws<InvalidOperationException>(() => report.AddTopicResult(Guid.NewGuid(), "Other", null, 1, 1, 1, 1, 101m, 3));
    }

    [Fact]
    public void AddGuidanceItem_RejectsDuplicateOrderInvalidSourceOrInvalidSnapshots()
    {
        var report = CreateReport();
        var topicId = Guid.NewGuid();

        report.AddGuidanceItem(topicId, PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.NewGuid(), "Title", "Metadata", 1);

        Assert.Throws<InvalidOperationException>(() => report.AddGuidanceItem(Guid.NewGuid(), PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.NewGuid(), "Other", null, 1));
        Assert.Throws<InvalidOperationException>(() => report.AddGuidanceItem(Guid.Empty, PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.NewGuid(), "Other", null, 2));
        Assert.Throws<InvalidOperationException>(() => report.AddGuidanceItem(Guid.NewGuid(), PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.Empty, "Other", null, 2));
        Assert.Throws<InvalidOperationException>(() => report.AddGuidanceItem(Guid.NewGuid(), PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.NewGuid(), " ", null, 2));
        Assert.Throws<InvalidOperationException>(() => report.AddGuidanceItem(Guid.NewGuid(), (PackageReportGuidanceSourceType)999, Guid.NewGuid(), "Other", null, 2));
    }

    [Fact]
    public void ChildCollections_AreReadOnlyExternally()
    {
        var report = CreateReport();
        report.AddTopicResult(Guid.NewGuid(), "Topic", null, 1, 1, 1, 1, 100m, 1);
        report.AddGuidanceItem(Guid.NewGuid(), PackageReportGuidanceSourceType.StudyMaterialVersion, Guid.NewGuid(), "Title", null, 1);

        Assert.IsAssignableFrom<IReadOnlyCollection<PackageAnalyticalReportTopicResult>>(report.TopicResults);
        Assert.IsAssignableFrom<IReadOnlyCollection<PackageAnalyticalReportGuidanceItem>>(report.GuidanceItems);
        Assert.False(report.TopicResults.GetType().IsGenericType && report.TopicResults.GetType().GetGenericTypeDefinition() == typeof(List<>));
        Assert.False(report.GuidanceItems.GetType().IsGenericType && report.GuidanceItems.GetType().GetGenericTypeDefinition() == typeof(List<>));
    }

    [Fact]
    public void PublicDomainPropertyNames_DoNotIncludeForbiddenSensitiveNames()
    {
        var forbiddenTerms = new[]
        {
            "QuestionText",
            "AnswerText",
            "CorrectAnswer",
            "CorrectOption",
            "AnswerKey",
            "Rationale",
            "PackageBenefitRightId",
            "PasswordHash",
            "AccessToken",
            "RefreshToken",
            "ProviderSession",
            "PaymentProvider"
        };
        var types = new[]
        {
            typeof(PackageAnalyticalReport),
            typeof(PackageAnalyticalReportTopicResult),
            typeof(PackageAnalyticalReportGuidanceItem)
        };

        var propertyNames = types.SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(property => property.Name));

        Assert.All(propertyNames, propertyName =>
        {
            Assert.DoesNotContain(forbiddenTerms, term => propertyName.Contains(term, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static PackageAnalyticalReport CreateReport(ReportFacts? facts = null)
    {
        facts ??= CreateReportFacts();

        return PackageAnalyticalReport.Create(
            facts.NurseProfileId,
            facts.ExamSessionId,
            facts.ExamSessionProvenanceId,
            facts.PackagePurchaseEntitlementId,
            facts.PackageOrderItemSnapshotId,
            facts.PaymentOrderId,
            facts.PaymentOrderItemId,
            facts.PreparationPackageDefinitionId,
            facts.PreparationPackageVersionId,
            facts.PreparationPackageOfferId,
            facts.IncludedExamId,
            facts.IncludedExamVersionId,
            facts.ReportingProfilePublicationId,
            facts.PracticeCollectionVersionId,
            facts.GeneratedAt,
            facts.FinalizedSessionStatus,
            facts.SubmittedAt,
            facts.FinalizedAt,
            facts.Score,
            facts.MaxScore,
            facts.Percentage,
            facts.Passed,
            facts.CorrectCount,
            facts.QuestionCount,
            facts.PackageAccessStartsAt,
            facts.PackageAccessEndsAt);
    }

    private static ReportFacts CreateReportFacts()
    {
        var finalizedAt = new DateTime(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc);
        return new ReportFacts(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            finalizedAt.AddMinutes(1),
            ExamSessionStatus.Submitted,
            finalizedAt,
            finalizedAt,
            8,
            10,
            80.00m,
            true,
            4,
            5,
            finalizedAt.AddDays(-30),
            finalizedAt.AddDays(60));
    }

    private sealed record ReportFacts(
        Guid NurseProfileId,
        Guid ExamSessionId,
        Guid ExamSessionProvenanceId,
        Guid PackagePurchaseEntitlementId,
        Guid PackageOrderItemSnapshotId,
        Guid PaymentOrderId,
        Guid PaymentOrderItemId,
        Guid PreparationPackageDefinitionId,
        Guid PreparationPackageVersionId,
        Guid PreparationPackageOfferId,
        Guid IncludedExamId,
        Guid IncludedExamVersionId,
        Guid ReportingProfilePublicationId,
        Guid PracticeCollectionVersionId,
        DateTime GeneratedAt,
        ExamSessionStatus FinalizedSessionStatus,
        DateTime? SubmittedAt,
        DateTime FinalizedAt,
        int Score,
        int MaxScore,
        decimal Percentage,
        bool Passed,
        int CorrectCount,
        int QuestionCount,
        DateTime PackageAccessStartsAt,
        DateTime PackageAccessEndsAt);
}
