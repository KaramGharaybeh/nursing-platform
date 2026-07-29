using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Domain.Tests.Exams;

public class ExamSessionProvenanceTests
{
    [Fact]
    public void ExamSessionProvenance_CreateForPackageAttempt_CapturesRequiredPackageFacts()
    {
        var facts = CreateFacts();

        var provenance = ExamSessionProvenance.CreateForPackageAttempt(
            facts.ExamSessionId,
            facts.PackagePurchaseEntitlementId,
            facts.PackageBenefitRightId,
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
            facts.PackageAccessStartsAt,
            facts.PackageAccessEndsAt,
            facts.StartedAt);

        Assert.NotEqual(Guid.Empty, provenance.Id);
        Assert.Equal(facts.ExamSessionId, provenance.ExamSessionId);
        Assert.Equal(facts.PackagePurchaseEntitlementId, provenance.PackagePurchaseEntitlementId);
        Assert.Equal(facts.PackageBenefitRightId, provenance.PackageBenefitRightId);
        Assert.Equal(facts.PackageOrderItemSnapshotId, provenance.PackageOrderItemSnapshotId);
        Assert.Equal(facts.PaymentOrderId, provenance.PaymentOrderId);
        Assert.Equal(facts.PaymentOrderItemId, provenance.PaymentOrderItemId);
        Assert.Equal(facts.PreparationPackageDefinitionId, provenance.PreparationPackageDefinitionId);
        Assert.Equal(facts.PreparationPackageVersionId, provenance.PreparationPackageVersionId);
        Assert.Equal(facts.PreparationPackageOfferId, provenance.PreparationPackageOfferId);
        Assert.Equal(facts.IncludedExamId, provenance.IncludedExamId);
        Assert.Equal(facts.IncludedExamVersionId, provenance.IncludedExamVersionId);
        Assert.Equal(facts.ReportingProfilePublicationId, provenance.ReportingProfilePublicationId);
        Assert.Equal(facts.PracticeCollectionVersionId, provenance.PracticeCollectionVersionId);
        Assert.Equal(facts.PackageAccessStartsAt, provenance.PackageAccessStartsAt);
        Assert.Equal(facts.PackageAccessEndsAt, provenance.PackageAccessEndsAt);
        Assert.Equal(facts.StartedAt, provenance.StartedAt);
    }

    [Fact]
    public void ExamSessionProvenance_CreateForPackageAttempt_RejectsMissingEntitlementRightOrSnapshotIds()
    {
        var facts = CreateFacts();

        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, examSessionId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, packagePurchaseEntitlementId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, packageBenefitRightId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, packageOrderItemSnapshotId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, paymentOrderId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, paymentOrderItemId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, preparationPackageDefinitionId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, preparationPackageVersionId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, preparationPackageOfferId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, includedExamId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, includedExamVersionId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, reportingProfilePublicationId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateWith(facts, practiceCollectionVersionId: Guid.Empty));
    }

    [Fact]
    public void ExamSessionProvenance_CreateForPackageAttempt_RejectsInvalidAccessWindow()
    {
        var facts = CreateFacts();

        Assert.Throws<InvalidOperationException>(() => ExamSessionProvenance.CreateForPackageAttempt(
            facts.ExamSessionId,
            facts.PackagePurchaseEntitlementId,
            facts.PackageBenefitRightId,
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
            facts.PackageAccessStartsAt,
            facts.PackageAccessStartsAt,
            facts.StartedAt));
    }

    private static ExamSessionProvenance CreateWith(
        ProvenanceFacts facts,
        Guid? examSessionId = null,
        Guid? packagePurchaseEntitlementId = null,
        Guid? packageBenefitRightId = null,
        Guid? packageOrderItemSnapshotId = null,
        Guid? paymentOrderId = null,
        Guid? paymentOrderItemId = null,
        Guid? preparationPackageDefinitionId = null,
        Guid? preparationPackageVersionId = null,
        Guid? preparationPackageOfferId = null,
        Guid? includedExamId = null,
        Guid? includedExamVersionId = null,
        Guid? reportingProfilePublicationId = null,
        Guid? practiceCollectionVersionId = null)
    {
        return ExamSessionProvenance.CreateForPackageAttempt(
            examSessionId ?? facts.ExamSessionId,
            packagePurchaseEntitlementId ?? facts.PackagePurchaseEntitlementId,
            packageBenefitRightId ?? facts.PackageBenefitRightId,
            packageOrderItemSnapshotId ?? facts.PackageOrderItemSnapshotId,
            paymentOrderId ?? facts.PaymentOrderId,
            paymentOrderItemId ?? facts.PaymentOrderItemId,
            preparationPackageDefinitionId ?? facts.PreparationPackageDefinitionId,
            preparationPackageVersionId ?? facts.PreparationPackageVersionId,
            preparationPackageOfferId ?? facts.PreparationPackageOfferId,
            includedExamId ?? facts.IncludedExamId,
            includedExamVersionId ?? facts.IncludedExamVersionId,
            reportingProfilePublicationId ?? facts.ReportingProfilePublicationId,
            practiceCollectionVersionId ?? facts.PracticeCollectionVersionId,
            facts.PackageAccessStartsAt,
            facts.PackageAccessEndsAt,
            facts.StartedAt);
    }

    private static ProvenanceFacts CreateFacts()
    {
        var startedAt = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        return new ProvenanceFacts(
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
            startedAt.AddDays(-1),
            startedAt.AddDays(89),
            startedAt);
    }

    private sealed record ProvenanceFacts(
        Guid ExamSessionId,
        Guid PackagePurchaseEntitlementId,
        Guid PackageBenefitRightId,
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
        DateTime PackageAccessStartsAt,
        DateTime PackageAccessEndsAt,
        DateTime StartedAt);
}
