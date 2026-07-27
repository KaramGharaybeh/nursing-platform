using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Domain.Tests.PreparationPackages;

public class PreparationPackageDomainTests
{
    [Fact]
    public void PublishedMaterialVersion_IsImmutableAfterPublish()
    {
        var version = StudyMaterialVersion.CreateDraft(
            Guid.NewGuid(),
            StudyMaterialType.FormattedText,
            "Draft content",
            null,
            null,
            null,
            [Guid.NewGuid()]);

        version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PublicationStatus.Published, version.Status);
        Assert.Throws<InvalidOperationException>(() => version.UpdateDraftContent(
            StudyMaterialType.FormattedText,
            "Changed content",
            null,
            null,
            null,
            [Guid.NewGuid()]));
    }

    [Fact]
    public void PublishedPracticeCollectionVersion_IsImmutableAfterPublish()
    {
        var version = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);
        version.AddPracticeItem(CreatePracticeItem());

        version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PublicationStatus.Published, version.Status);
        Assert.Throws<InvalidOperationException>(() => version.AddPracticeItem(CreatePracticeItem()));
    }

    [Fact]
    public void PublishedReportingProfile_IsImmutableAfterPublish()
    {
        var profile = ReportingProfilePublication.CreateDraft(Guid.NewGuid(), "NCLEX profile");
        profile.AssignQuestion(Guid.NewGuid(), Guid.NewGuid());

        profile.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PublicationStatus.Published, profile.Status);
        Assert.Throws<InvalidOperationException>(() => profile.AssignQuestion(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void PublishedPackageVersion_IsImmutableAfterPublish()
    {
        var version = PreparationPackageVersion.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        version.AddMaterialVersion(Guid.NewGuid(), 1);
        version.ConfirmContentIsolation();

        version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PreparationPackageVersionStatus.Published, version.Status);
        Assert.Throws<InvalidOperationException>(() => version.AddMaterialVersion(Guid.NewGuid(), 2));
    }

    [Fact]
    public void PackageVersion_CannotPublishWithoutMaterials()
    {
        var version = PreparationPackageVersion.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        version.ConfirmContentIsolation();

        Assert.Throws<InvalidOperationException>(() => version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageVersion_CannotPublishWithoutContentIsolationConfirmation()
    {
        var version = PreparationPackageVersion.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        version.AddMaterialVersion(Guid.NewGuid(), 1);

        Assert.Throws<InvalidOperationException>(() => version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageVersion_ReferencesExactlyOneExamVersionOneReportingProfileAndOnePracticeCollectionVersion()
    {
        var packageDefinitionId = Guid.NewGuid();
        var examVersionId = Guid.NewGuid();
        var reportingProfilePublicationId = Guid.NewGuid();
        var practiceCollectionVersionId = Guid.NewGuid();

        var version = PreparationPackageVersion.CreateDraft(
            packageDefinitionId,
            examVersionId,
            reportingProfilePublicationId,
            practiceCollectionVersionId);

        Assert.Equal(packageDefinitionId, version.PreparationPackageDefinitionId);
        Assert.Equal(examVersionId, version.ExamVersionId);
        Assert.Equal(reportingProfilePublicationId, version.ReportingProfilePublicationId);
        Assert.Equal(practiceCollectionVersionId, version.PracticeCollectionVersionId);
    }

    [Fact]
    public void PackageVersion_MaterialsHaveDeterministicPositiveOrdering()
    {
        var version = PreparationPackageVersion.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var secondMaterialVersionId = Guid.NewGuid();
        var firstMaterialVersionId = Guid.NewGuid();

        version.AddMaterialVersion(secondMaterialVersionId, 2);
        version.AddMaterialVersion(firstMaterialVersionId, 1);

        Assert.Collection(
            version.GetOrderedMaterials(),
            first =>
            {
                Assert.Equal(firstMaterialVersionId, first.StudyMaterialVersionId);
                Assert.Equal(1, first.SortOrder);
            },
            second =>
            {
                Assert.Equal(secondMaterialVersionId, second.StudyMaterialVersionId);
                Assert.Equal(2, second.SortOrder);
            });
        Assert.Throws<InvalidOperationException>(() => version.AddMaterialVersion(Guid.NewGuid(), 0));
        Assert.Throws<InvalidOperationException>(() => version.AddMaterialVersion(Guid.NewGuid(), 1));
    }

    [Fact]
    public void Offer_CarriesCommercialConfigurationWithoutComponentSelection()
    {
        var packageDefinitionId = Guid.NewGuid();
        var packageVersionId = Guid.NewGuid();

        var offer = PreparationPackageOffer.CreateDraft(
            packageDefinitionId,
            packageVersionId,
            "NCLEX preparation package",
            "nclex-preparation-package",
            "Focused preparation materials and practice.",
            14900,
            "usd",
            90);

        Assert.Equal(packageDefinitionId, offer.PreparationPackageDefinitionId);
        Assert.Equal(packageVersionId, offer.PreparationPackageVersionId);
        Assert.Equal("NCLEX preparation package", offer.Title);
        Assert.Equal("nclex-preparation-package", offer.Slug);
        Assert.Equal("Focused preparation materials and practice.", offer.Summary);
        Assert.Equal(14900, offer.PriceAmountMinor);
        Assert.Equal("USD", offer.Currency);
        Assert.Equal(90, offer.AccessDurationDays);
        Assert.Equal(PreparationPackageOfferStatus.Draft, offer.Status);
        Assert.DoesNotContain(offer.GetType().GetProperties(), property => property.Name.Contains("ExamVersion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(offer.GetType().GetProperties(), property => property.Name.Contains("ReportingProfile", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(offer.GetType().GetProperties(), property => property.Name.Contains("MaterialVersion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(offer.GetType().GetProperties(), property => property.Name.Contains("PracticeCollectionVersion", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PracticeCollectionVersion_CannotPublishWithoutPracticeItems()
    {
        var version = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);

        Assert.Throws<InvalidOperationException>(() => version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void ReportingProfilePublication_CannotPublishWithoutQuestionAssignments()
    {
        var profile = ReportingProfilePublication.CreateDraft(Guid.NewGuid(), "NCLEX profile");

        Assert.Throws<InvalidOperationException>(() => profile.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void RequiredGuidIdentities_CannotBeEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => PreparationPackageDefinition.Create(Guid.Empty, Guid.NewGuid(), "Package", "package", null));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageDefinition.Create(Guid.NewGuid(), Guid.Empty, "Package", "package", null));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageVersion.CreateDraft(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageVersion.CreateDraft(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageVersion.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageVersion.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty));

        var packageVersion = PreparationPackageVersion.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => packageVersion.AddMaterialVersion(Guid.Empty, 1));

        Assert.Throws<InvalidOperationException>(() => PreparationPackageOffer.CreateDraft(Guid.Empty, Guid.NewGuid(), "Offer", "offer", null, 1000, "USD", 30));
        Assert.Throws<InvalidOperationException>(() => PreparationPackageOffer.CreateDraft(Guid.NewGuid(), Guid.Empty, "Offer", "offer", null, 1000, "USD", 30));
        Assert.Throws<InvalidOperationException>(() => StudyMaterialVersion.CreateDraft(Guid.Empty, StudyMaterialType.FormattedText, "content", null, null, null, [Guid.NewGuid()]));
        Assert.Throws<InvalidOperationException>(() => StudyMaterialVersion.CreateDraft(Guid.NewGuid(), StudyMaterialType.FormattedText, "content", null, null, null, [Guid.Empty]));
        Assert.Throws<InvalidOperationException>(() => PracticeCollectionVersion.CreateDraft(Guid.Empty, 1));
        Assert.Throws<InvalidOperationException>(() => PracticeItem.Create(Guid.Empty, "Prompt", "Feedback", 1));
        Assert.Throws<InvalidOperationException>(() => ReportingTopic.Create(Guid.Empty, "Topic", "topic", null));
        Assert.Throws<InvalidOperationException>(() => ReportingProfilePublication.CreateDraft(Guid.Empty, "Profile"));

        var profile = ReportingProfilePublication.CreateDraft(Guid.NewGuid(), "Profile");
        Assert.Throws<InvalidOperationException>(() => profile.AssignQuestion(Guid.Empty, Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => profile.AssignQuestion(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void PracticeCollectionVersion_CannotPublishItemWithoutCorrectAnswer()
    {
        var version = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);
        var item = PracticeItem.Create(Guid.NewGuid(), "Independent practice prompt", "Independent feedback", 1);
        item.AddAnswerOption("Incorrect option one", false, 1);
        item.AddAnswerOption("Incorrect option two", false, 2);
        version.AddPracticeItem(item);

        Assert.Throws<InvalidOperationException>(() => version.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc)));
    }

    [Theory]
    [InlineData(StudyMaterialType.FormattedText, null, null, null, null)]
    [InlineData(StudyMaterialType.File, null, null, null, null)]
    [InlineData(StudyMaterialType.ExternalLink, null, null, null, null)]
    [InlineData(StudyMaterialType.Video, null, null, null, null)]
    [InlineData(StudyMaterialType.FormattedText, "content", "file-key", null, null)]
    [InlineData(StudyMaterialType.File, null, "file-key", "https://example.test", null)]
    [InlineData(StudyMaterialType.ExternalLink, null, null, "https://example.test", "https://video.example.test")]
    [InlineData(StudyMaterialType.Video, "content", null, null, "https://video.example.test")]
    public void StudyMaterialVersion_ContentMustMatchMaterialType(
        StudyMaterialType materialType,
        string? formattedTextContent,
        string? fileStorageKey,
        string? externalUrl,
        string? videoUrl)
    {
        Assert.Throws<InvalidOperationException>(() => StudyMaterialVersion.CreateDraft(
            Guid.NewGuid(),
            materialType,
            formattedTextContent,
            fileStorageKey,
            externalUrl,
            videoUrl,
            [Guid.NewGuid()]));
    }

    private static PracticeItem CreatePracticeItem()
    {
        var item = PracticeItem.Create(
            Guid.NewGuid(),
            "Independent practice prompt",
            "Independent practice feedback",
            1);

        item.AddAnswerOption("Correct option", true, 1);
        item.AddAnswerOption("Incorrect option", false, 2);

        return item;
    }
}
