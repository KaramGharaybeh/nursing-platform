using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.PreparationPackages.Admin.PackageDefinitions;
using NursingPlatform.Application.PreparationPackages.Admin.PackageOffers;
using NursingPlatform.Application.PreparationPackages.Admin.PackageVersions;
using NursingPlatform.Application.PreparationPackages.Admin.PracticeCollections;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingTopics;
using NursingPlatform.Application.PreparationPackages.Admin.StudyMaterials;
using NursingPlatform.Application.PreparationPackages.Catalog;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.PreparationPackages;

public class PreparationPackageHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_CreateUpdateArchiveReportingTopic_ManagesActiveTopic()
    {
        var category = CreateCategory();
        var topics = new List<ReportingTopic>();
        SetupContext(categories: [category], topics: topics);

        var create = new CreateAdminReportingTopicCommandHandler(_contextMock.Object);
        var created = await create.Handle(new CreateAdminReportingTopicCommand
        {
            Request = new CreateAdminReportingTopicRequest { ExamCategoryId = category.Id, Name = "Pharmacology", Slug = "pharmacology" }
        }, CancellationToken.None);

        var update = new UpdateAdminReportingTopicCommandHandler(_contextMock.Object);
        var updated = await update.Handle(new UpdateAdminReportingTopicCommand
        {
            Id = created.Id,
            Request = new UpdateAdminReportingTopicRequest { ExamCategoryId = category.Id, Name = "Updated", Slug = "updated" }
        }, CancellationToken.None);

        var archive = new ArchiveAdminReportingTopicCommandHandler(_contextMock.Object);
        var archived = await archive.Handle(new ArchiveAdminReportingTopicCommand { Id = created.Id }, CancellationToken.None);

        Assert.Equal("Updated", updated.Name);
        Assert.False(archived.IsActive);
        Assert.Single(topics);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_PublishReportingProfile_WhenAssignmentsAreEmpty_ThrowsInvalidOperationException()
    {
        var exam = CreateExam();
        var version = CreateExamVersion(exam.Id, ExamVersionStatus.Published);
        var profile = ReportingProfilePublication.CreateDraft(version.Id, "Profile");
        SetupContext(exams: [exam], versions: [version], profiles: [profile]);
        var handler = new PublishAdminReportingProfileCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminReportingProfileCommand
        {
            Id = profile.Id,
            Request = new PublishAdminReportingProfileRequest { Assignments = [] }
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PublishReportingProfile_WhenExamVersionIsNotPublished_ThrowsInvalidOperationException()
    {
        var exam = CreateExam();
        var version = CreateExamVersion(exam.Id, ExamVersionStatus.Draft);
        var question = CreateQuestion(version.Id);
        var topic = ReportingTopic.Create(exam.ExamCategoryId!.Value, "Topic", "topic", null);
        var profile = ReportingProfilePublication.CreateDraft(version.Id, "Profile");
        SetupContext(exams: [exam], versions: [version], questions: [question], topics: [topic], profiles: [profile]);
        var handler = new PublishAdminReportingProfileCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminReportingProfileCommand
        {
            Id = profile.Id,
            Request = new PublishAdminReportingProfileRequest
            {
                Assignments = [new ReportingProfileQuestionAssignmentRequest { ExamQuestionId = question.Id, ReportingTopicId = topic.Id }]
            }
        }, CancellationToken.None));

        Assert.Equal("Reporting profile publication requires a published exam version.", exception.Message);
        Assert.Equal(PublicationStatus.Draft, profile.Status);
        Assert.Empty(profile.Assignments);
    }

    [Fact]
    public async Task Handle_PublishReportingProfile_WhenPublishedProfileAlreadyExistsForExamVersion_ThrowsInvalidOperationException()
    {
        var exam = CreateExam();
        var version = CreateExamVersion(exam.Id, ExamVersionStatus.Published);
        var question = CreateQuestion(version.Id);
        var topic = ReportingTopic.Create(exam.ExamCategoryId!.Value, "Topic", "topic", null);
        var existing = ReportingProfilePublication.CreateDraft(version.Id, "Existing profile");
        existing.AssignQuestion(question.Id, topic.Id);
        existing.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));
        var draft = ReportingProfilePublication.CreateDraft(version.Id, "Replacement profile");
        SetupContext(exams: [exam], versions: [version], questions: [question], topics: [topic], profiles: [existing, draft]);
        var handler = new PublishAdminReportingProfileCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminReportingProfileCommand
        {
            Id = draft.Id,
            Request = new PublishAdminReportingProfileRequest
            {
                Assignments = [new ReportingProfileQuestionAssignmentRequest { ExamQuestionId = question.Id, ReportingTopicId = topic.Id }]
            }
        }, CancellationToken.None));

        Assert.Equal("A published reporting profile already exists for this exam version.", exception.Message);
        Assert.Equal(PublicationStatus.Draft, draft.Status);
        Assert.Empty(draft.Assignments);
    }

    [Fact]
    public async Task Handle_PublishReportingProfile_WhenTopicCategoryMismatch_ThrowsInvalidOperationException()
    {
        var exam = CreateExam();
        var version = CreateExamVersion(exam.Id, ExamVersionStatus.Published);
        var question = CreateQuestion(version.Id);
        var profile = ReportingProfilePublication.CreateDraft(version.Id, "Profile");
        var topic = ReportingTopic.Create(Guid.NewGuid(), "Other", "other", null);
        SetupContext(exams: [exam], versions: [version], questions: [question], topics: [topic], profiles: [profile]);
        var handler = new PublishAdminReportingProfileCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminReportingProfileCommand
        {
            Id = profile.Id,
            Request = new PublishAdminReportingProfileRequest
            {
                Assignments = [new ReportingProfileQuestionAssignmentRequest { ExamQuestionId = question.Id, ReportingTopicId = topic.Id }]
            }
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PublishMaterialVersion_WhenTopicsMissing_ThrowsInvalidOperationException()
    {
        var material = StudyMaterial.Create("Material", "material", null);
        var version = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Content", null, null, null, [Guid.NewGuid()]);
        SetupContext(materials: [material], materialVersions: [version], topics: []);
        var handler = new PublishAdminStudyMaterialVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminStudyMaterialVersionCommand
        {
            StudyMaterialId = material.Id,
            VersionId = version.Id
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreateMaterialVersion_WhenReportingTopicIsMissing_ThrowsInvalidOperationException()
    {
        var material = StudyMaterial.Create("Material", "material", null);
        var versions = new List<StudyMaterialVersion>();
        SetupContext(materials: [material], materialVersions: versions, topics: []);
        var handler = new CreateAdminStudyMaterialVersionCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateAdminStudyMaterialVersionCommand
        {
            StudyMaterialId = material.Id,
            Request = new CreateAdminStudyMaterialVersionRequest
            {
                MaterialType = StudyMaterialType.FormattedText,
                FormattedTextContent = "Study content",
                ReportingTopicIds = [Guid.NewGuid()]
            }
        }, CancellationToken.None));

        Assert.Equal("Material version topics must exist and be active.", exception.Message);
        Assert.Empty(versions);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateMaterialVersion_WhenReportingTopicIsMissing_ThrowsInvalidOperationExceptionAndPreservesDraft()
    {
        var material = StudyMaterial.Create("Material", "material", null);
        var originalTopic = ReportingTopic.Create(Guid.NewGuid(), "Original", "original", null);
        var version = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Original content", null, null, null, [originalTopic.Id]);
        SetupContext(materials: [material], materialVersions: [version], topics: [originalTopic]);
        var handler = new UpdateAdminStudyMaterialVersionCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new UpdateAdminStudyMaterialVersionCommand
        {
            StudyMaterialId = material.Id,
            VersionId = version.Id,
            Request = new UpdateAdminStudyMaterialVersionRequest
            {
                MaterialType = StudyMaterialType.ExternalLink,
                ExternalUrl = "https://example.test/study",
                ReportingTopicIds = [Guid.NewGuid()]
            }
        }, CancellationToken.None));

        Assert.Equal("Material version topics must exist and be active.", exception.Message);
        Assert.Equal(StudyMaterialType.FormattedText, version.MaterialType);
        Assert.Equal("Original content", version.FormattedTextContent);
        Assert.Null(version.ExternalUrl);
        Assert.Equal(originalTopic.Id, Assert.Single(version.Topics).ReportingTopicId);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreateMaterialVersion_AfterPublishedVersionCreatesNextDraftWithoutMutatingPublishedVersion()
    {
        var material = StudyMaterial.Create("Material", "material", null);
        var topic = ReportingTopic.Create(Guid.NewGuid(), "Topic", "topic", null);
        var published = StudyMaterialVersion.CreateDraft(material.Id, StudyMaterialType.FormattedText, "Published content", null, null, null, [topic.Id]);
        published.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));
        var versions = new List<StudyMaterialVersion> { published };
        SetupContext(materials: [material], materialVersions: versions, topics: [topic]);
        var handler = new CreateAdminStudyMaterialVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminStudyMaterialVersionCommand
        {
            StudyMaterialId = material.Id,
            Request = new CreateAdminStudyMaterialVersionRequest
            {
                MaterialType = StudyMaterialType.ExternalLink,
                ExternalUrl = "https://example.test/revision",
                ReportingTopicIds = [topic.Id]
            }
        }, CancellationToken.None);

        Assert.Equal(2, result.VersionNumber);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(PublicationStatus.Published, published.Status);
        Assert.Equal("Published content", published.FormattedTextContent);
        Assert.Equal(topic.Id, Assert.Single(published.Topics).ReportingTopicId);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public async Task Handle_PublishPracticeCollectionVersion_WhenCollectionIsEmpty_ThrowsInvalidOperationException()
    {
        var collection = PracticeCollection.Create("Practice", "practice", null);
        var version = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        SetupContext(practiceCollections: [collection], practiceVersions: [version]);
        var handler = new PublishAdminPracticeCollectionVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collection.Id,
            VersionId = version.Id
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreatePracticeCollectionVersion_WhenReportingTopicIsMissing_ThrowsInvalidOperationException()
    {
        var collection = PracticeCollection.Create("Practice", "practice", null);
        var versions = new List<PracticeCollectionVersion>();
        SetupContext(practiceCollections: [collection], practiceVersions: versions, topics: []);
        var handler = new CreateAdminPracticeCollectionVersionCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collection.Id,
            Request = new CreateAdminPracticeCollectionVersionRequest
            {
                Items =
                [
                    new UpsertAdminPracticeItemRequest
                    {
                        ReportingTopicId = Guid.NewGuid(),
                        Prompt = "Independent practice prompt",
                        ImmediateFeedback = "Independent practice feedback",
                        DisplayOrder = 1,
                        AnswerOptions =
                        [
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Correct", IsCorrect = true, DisplayOrder = 1 },
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Incorrect", IsCorrect = false, DisplayOrder = 2 }
                        ]
                    }
                ]
            }
        }, CancellationToken.None));

        Assert.Equal("Practice item topics must exist and be active.", exception.Message);
        Assert.Empty(versions);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatePracticeCollectionVersion_WhenReportingTopicIsInactive_ThrowsInvalidOperationExceptionAndPreservesDraft()
    {
        var collection = PracticeCollection.Create("Practice", "practice", null);
        var originalTopic = ReportingTopic.Create(Guid.NewGuid(), "Original", "original", null);
        var inactiveTopic = ReportingTopic.Create(originalTopic.ExamCategoryId, "Inactive", "inactive", null);
        inactiveTopic.Archive();
        var version = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        var original = PracticeItem.Create(originalTopic.Id, "Original prompt", "Original feedback", 1);
        original.AddAnswerOption("A", true, 1);
        original.AddAnswerOption("B", false, 2);
        version.AddPracticeItem(original);
        SetupContext(topics: [originalTopic, inactiveTopic], practiceCollections: [collection], practiceVersions: [version]);
        var handler = new UpdateAdminPracticeCollectionVersionCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new UpdateAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collection.Id,
            VersionId = version.Id,
            Request = new UpdateAdminPracticeCollectionVersionRequest
            {
                Items =
                [
                    new UpsertAdminPracticeItemRequest
                    {
                        ReportingTopicId = inactiveTopic.Id,
                        Prompt = "Replacement prompt",
                        ImmediateFeedback = "Replacement feedback",
                        DisplayOrder = 1,
                        AnswerOptions =
                        [
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Correct", IsCorrect = true, DisplayOrder = 1 },
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Incorrect", IsCorrect = false, DisplayOrder = 2 }
                        ]
                    }
                ]
            }
        }, CancellationToken.None));

        Assert.Equal("Practice item topics must exist and be active.", exception.Message);
        var item = Assert.Single(version.Items);
        Assert.Equal(originalTopic.Id, item.ReportingTopicId);
        Assert.Equal("Original prompt", item.Prompt);
        Assert.Equal("Original feedback", item.ImmediateFeedback);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreatePracticeCollectionVersion_AfterPublishedVersionCreatesNextDraftWithoutMutatingPublishedVersion()
    {
        var collection = PracticeCollection.Create("Practice", "practice", null);
        var topic = ReportingTopic.Create(Guid.NewGuid(), "Topic", "topic", null);
        var published = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        var publishedItem = PracticeItem.Create(topic.Id, "Published prompt", "Published feedback", 1);
        publishedItem.AddAnswerOption("Published correct", true, 1);
        publishedItem.AddAnswerOption("Published incorrect", false, 2);
        published.AddPracticeItem(publishedItem);
        published.Publish(new DateTime(2026, 7, 27, 9, 0, 0, DateTimeKind.Utc));
        var versions = new List<PracticeCollectionVersion> { published };
        SetupContext(topics: [topic], practiceCollections: [collection], practiceVersions: versions);
        var handler = new CreateAdminPracticeCollectionVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new CreateAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collection.Id,
            Request = new CreateAdminPracticeCollectionVersionRequest
            {
                Items =
                [
                    new UpsertAdminPracticeItemRequest
                    {
                        ReportingTopicId = topic.Id,
                        Prompt = "Revision prompt",
                        ImmediateFeedback = "Revision feedback",
                        DisplayOrder = 1,
                        AnswerOptions =
                        [
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Revision correct", IsCorrect = true, DisplayOrder = 1 },
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Revision incorrect", IsCorrect = false, DisplayOrder = 2 }
                        ]
                    }
                ]
            }
        }, CancellationToken.None);

        Assert.Equal(2, result.VersionNumber);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(PublicationStatus.Published, published.Status);
        var unchangedItem = Assert.Single(published.Items);
        Assert.Equal(topic.Id, unchangedItem.ReportingTopicId);
        Assert.Equal("Published prompt", unchangedItem.Prompt);
        Assert.Equal("Published feedback", unchangedItem.ImmediateFeedback);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public async Task Handle_UpdatePracticeCollectionVersion_WhenDraft_ReplacesPracticeItems()
    {
        var collection = PracticeCollection.Create("Practice", "practice", null);
        var topic = ReportingTopic.Create(Guid.NewGuid(), "Topic", "topic", null);
        var version = PracticeCollectionVersion.CreateDraft(collection.Id, 1);
        var original = PracticeItem.Create(topic.Id, "Original prompt", "Original feedback", 1);
        original.AddAnswerOption("A", true, 1);
        original.AddAnswerOption("B", false, 2);
        version.AddPracticeItem(original);
        SetupContext(topics: [topic], practiceCollections: [collection], practiceVersions: [version]);
        var handler = new UpdateAdminPracticeCollectionVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new UpdateAdminPracticeCollectionVersionCommand
        {
            PracticeCollectionId = collection.Id,
            VersionId = version.Id,
            Request = new UpdateAdminPracticeCollectionVersionRequest
            {
                Items =
                [
                    new UpsertAdminPracticeItemRequest
                    {
                        ReportingTopicId = topic.Id,
                        Prompt = "Replacement prompt",
                        ImmediateFeedback = "Replacement feedback",
                        DisplayOrder = 1,
                        AnswerOptions =
                        [
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Correct", IsCorrect = true, DisplayOrder = 1 },
                            new UpsertAdminPracticeAnswerOptionRequest { OptionText = "Incorrect", IsCorrect = false, DisplayOrder = 2 }
                        ]
                    }
                ]
            }
        }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("Replacement prompt", item.Prompt);
        Assert.Single(version.Items);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PublishPackageVersion_WhenMaterialIsMissing_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph(includeMaterial: false);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.ConfirmContentIsolation();
        SetupContext(definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion]);
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.Definition.Id,
            VersionId = packageVersion.Id
        }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PublishPackageVersion_WhenReferencedMaterialVersionIsDraft_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph();
        var draftMaterial = StudyMaterialVersion.CreateDraft(graph.MaterialVersion!.StudyMaterialId, StudyMaterialType.FormattedText, "Draft content", null, null, null, [graph.Topic.Id], 2);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(draftMaterial.Id, 1);
        packageVersion.ConfirmContentIsolation();
        SetupContext(definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [draftMaterial], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion]);
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.Definition.Id,
            VersionId = packageVersion.Id
        }, CancellationToken.None));

        Assert.Equal(PreparationPackageVersionStatus.Draft, packageVersion.Status);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PublishPackageVersion_WhenReferencedMaterialVersionIsRetired_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph();
        graph.MaterialVersion!.Retire(DateTime.UtcNow);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion.Id, 1);
        packageVersion.ConfirmContentIsolation();
        SetupContext(definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [graph.MaterialVersion], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion]);
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.Definition.Id,
            VersionId = packageVersion.Id
        }, CancellationToken.None));

        Assert.Equal(PreparationPackageVersionStatus.Draft, packageVersion.Status);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PublishPackageVersion_WhenReferencedPracticeCollectionVersionIsDraft_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph();
        var draftPracticeVersion = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 2);
        var practiceItem = PracticeItem.Create(graph.Topic.Id, "Draft practice prompt", "Draft practice feedback", 1);
        practiceItem.AddAnswerOption("Correct", true, 1);
        practiceItem.AddAnswerOption("Incorrect", false, 2);
        draftPracticeVersion.AddPracticeItem(practiceItem);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, draftPracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion!.Id, 1);
        packageVersion.ConfirmContentIsolation();
        SetupContext(definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [graph.MaterialVersion], practiceVersions: [draftPracticeVersion], packageVersions: [packageVersion]);
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.Definition.Id,
            VersionId = packageVersion.Id
        }, CancellationToken.None));

        Assert.Equal(PreparationPackageVersionStatus.Draft, packageVersion.Status);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PublishPackageVersion_WhenComponentsAreValid_PublishesVersion()
    {
        var graph = CreateValidPackageGraph();
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion!.Id, 1);
        packageVersion.ConfirmContentIsolation();
        SetupContext(definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [graph.MaterialVersion], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion]);
        var handler = new PublishAdminPreparationPackageVersionCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new PublishAdminPreparationPackageVersionCommand
        {
            PreparationPackageDefinitionId = graph.Definition.Id,
            VersionId = packageVersion.Id
        }, CancellationToken.None);

        Assert.Equal("Published", result.Status);
        Assert.NotNull(packageVersion.PublishedAt);
    }

    [Fact]
    public async Task Handle_ActivateOffer_WhenPackageVersionIsUnpublished_ThrowsInvalidOperationException()
    {
        var definition = PreparationPackageDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Package", "package", null);
        var version = PreparationPackageVersion.CreateDraft(definition.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var offer = PreparationPackageOffer.CreateDraft(definition.Id, version.Id, "Offer", "offer", null, 1000, "USD", 30);
        SetupContext(definitions: [definition], packageVersions: [version], offers: [offer]);
        var handler = new ActivateAdminPreparationPackageOfferCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new ActivateAdminPreparationPackageOfferCommand { Id = offer.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ActivateOffer_WhenReferencedMaterialVersionIsRetired_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph();
        graph.MaterialVersion!.Retire(DateTime.UtcNow);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion.Id, 1);
        packageVersion.ConfirmContentIsolation();
        packageVersion.Publish(DateTime.UtcNow);
        var offer = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Offer", "offer", null, 1000, "USD", 30);
        SetupContext(
            definitions: [graph.Definition],
            exams: [graph.Exam],
            versions: [graph.ExamVersion],
            questions: graph.Questions,
            topics: [graph.Topic],
            profiles: [graph.Profile],
            materialVersions: [graph.MaterialVersion],
            practiceVersions: [graph.PracticeVersion],
            packageVersions: [packageVersion],
            offers: [offer]);
        var handler = new ActivateAdminPreparationPackageOfferCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new ActivateAdminPreparationPackageOfferCommand { Id = offer.Id }, CancellationToken.None));

        Assert.Equal(PreparationPackageOfferStatus.Draft, offer.Status);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActivateOffer_WhenAnotherOfferIsActiveForSamePackageDefinition_ThrowsInvalidOperationException()
    {
        var graph = CreateValidPackageGraph();
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion!.Id, 1);
        packageVersion.ConfirmContentIsolation();
        packageVersion.Publish(DateTime.UtcNow);
        var active = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Active", "active", null, 1000, "USD", 30);
        active.Activate(DateTime.UtcNow);
        var draft = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Draft", "draft", null, 1000, "USD", 30);
        SetupContext(packageVersions: [packageVersion], offers: [active, draft]);
        var handler = new ActivateAdminPreparationPackageOfferCommandHandler(_contextMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new ActivateAdminPreparationPackageOfferCommand { Id = draft.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ListCatalogOffers_ReturnsOnlyActiveEligibleOffersWithSafeSellingFields()
    {
        var graph = CreateValidPackageGraph();
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion!.Id, 1);
        packageVersion.ConfirmContentIsolation();
        packageVersion.Publish(DateTime.UtcNow);
        var active = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Active", "active", null, 1000, "USD", 30);
        active.Activate(DateTime.UtcNow);
        var inactive = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Inactive", "inactive", null, 1000, "USD", 30);
        SetupContext(countries: [graph.Country], categories: [graph.Category], definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [graph.MaterialVersion], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion], offers: [active, inactive]);
        var handler = new ListPreparationPackageOffersQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListPreparationPackageOffersQuery { Page = 1, PageSize = 10 }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("Active", item.Title);
        Assert.Equal(graph.Exam.Title, item.ExamTitle);
        Assert.DoesNotContain(item.GetType().GetProperties(), property => property.Name.Contains("Question", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(item.GetType().GetProperties(), property => property.Name.Contains("Answer", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_ListCatalogOffers_ExcludesActiveOfferWhenPublishedComponentIsNoLongerEligible()
    {
        var graph = CreateValidPackageGraph();
        graph.MaterialVersion!.Retire(DateTime.UtcNow);
        var packageVersion = PreparationPackageVersion.CreateDraft(graph.Definition.Id, graph.ExamVersion.Id, graph.Profile.Id, graph.PracticeVersion.Id);
        packageVersion.AddMaterialVersion(graph.MaterialVersion.Id, 1);
        packageVersion.ConfirmContentIsolation();
        packageVersion.Publish(DateTime.UtcNow);
        var active = PreparationPackageOffer.CreateDraft(graph.Definition.Id, packageVersion.Id, "Active", "active", null, 1000, "USD", 30);
        active.Activate(DateTime.UtcNow);
        SetupContext(countries: [graph.Country], categories: [graph.Category], definitions: [graph.Definition], exams: [graph.Exam], versions: [graph.ExamVersion], questions: graph.Questions, topics: [graph.Topic], profiles: [graph.Profile], materialVersions: [graph.MaterialVersion], practiceVersions: [graph.PracticeVersion], packageVersions: [packageVersion], offers: [active]);
        var handler = new ListPreparationPackageOffersQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListPreparationPackageOffersQuery { Page = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Empty(result.Items);
    }

    private void SetupContext(
        IReadOnlyCollection<Country>? countries = null,
        IReadOnlyCollection<ExamCategory>? categories = null,
        IReadOnlyCollection<Exam>? exams = null,
        IReadOnlyCollection<ExamVersion>? versions = null,
        IReadOnlyCollection<ExamQuestion>? questions = null,
        IReadOnlyCollection<ReportingTopic>? topics = null,
        IReadOnlyCollection<ReportingProfilePublication>? profiles = null,
        IReadOnlyCollection<StudyMaterial>? materials = null,
        IReadOnlyCollection<StudyMaterialVersion>? materialVersions = null,
        IReadOnlyCollection<PracticeCollection>? practiceCollections = null,
        IReadOnlyCollection<PracticeCollectionVersion>? practiceVersions = null,
        IReadOnlyCollection<PreparationPackageDefinition>? definitions = null,
        IReadOnlyCollection<PreparationPackageVersion>? packageVersions = null,
        IReadOnlyCollection<PreparationPackageOffer>? offers = null)
    {
        SetupDbSet(c => c.Countries, ToMutableList(countries));
        SetupDbSet(c => c.ExamCategories, ToMutableList(categories));
        SetupDbSet(c => c.Exams, ToMutableList(exams));
        SetupDbSet(c => c.ExamVersions, ToMutableList(versions));
        SetupDbSet(c => c.ExamQuestions, ToMutableList(questions));
        SetupDbSet(c => c.ReportingTopics, ToMutableList(topics));
        SetupDbSet(c => c.ReportingProfilePublications, ToMutableList(profiles));
        SetupDbSet(c => c.ReportingProfileQuestionAssignments, profiles?.SelectMany(p => p.Assignments).ToList() ?? []);
        SetupDbSet(c => c.StudyMaterials, ToMutableList(materials));
        SetupDbSet(c => c.StudyMaterialVersions, ToMutableList(materialVersions));
        SetupDbSet(c => c.StudyMaterialVersionTopics, materialVersions?.SelectMany(v => v.Topics).ToList() ?? []);
        SetupDbSet(c => c.PracticeCollections, ToMutableList(practiceCollections));
        SetupDbSet(c => c.PracticeCollectionVersions, ToMutableList(practiceVersions));
        SetupDbSet(c => c.PracticeItems, practiceVersions?.SelectMany(v => v.Items).ToList() ?? []);
        SetupDbSet(c => c.PracticeAnswerOptions, practiceVersions?.SelectMany(v => v.Items).SelectMany(i => i.AnswerOptions).ToList() ?? []);
        SetupDbSet(c => c.PreparationPackageDefinitions, ToMutableList(definitions));
        SetupDbSet(c => c.PreparationPackageVersions, ToMutableList(packageVersions));
        SetupDbSet(c => c.PreparationPackageVersionMaterials, packageVersions?.SelectMany(v => v.Materials).ToList() ?? []);
        SetupDbSet(c => c.PreparationPackageOffers, ToMutableList(offers));
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static List<T> ToMutableList<T>(IReadOnlyCollection<T>? items)
    {
        return items as List<T> ?? items?.ToList() ?? [];
    }

    private void SetupDbSet<T>(System.Linq.Expressions.Expression<Func<IApplicationDbContext, Microsoft.EntityFrameworkCore.DbSet<T>>> expression, List<T> items)
        where T : class
    {
        var dbSet = items.AsQueryable().BuildMockDbSet();
        dbSet.Setup(s => s.Add(It.IsAny<T>())).Callback<T>(items.Add);
        _contextMock.Setup(expression).Returns(dbSet.Object);
    }

    private static Country CreateCountry(Guid? id = null) => new() { Id = id ?? Guid.NewGuid(), Name = "United States", Code = "US", IsActive = true };

    private static ExamCategory CreateCategory(Guid? id = null, Guid? countryId = null) => new() { Id = id ?? Guid.NewGuid(), CountryId = countryId ?? Guid.NewGuid(), Name = "NCLEX", Slug = "nclex", IsActive = true };

    private static Exam CreateExam(Guid? countryId = null, Guid? categoryId = null) => new() { Id = Guid.NewGuid(), CountryId = countryId ?? Guid.NewGuid(), ExamCategoryId = categoryId ?? Guid.NewGuid(), Title = "NCLEX RN", Slug = "nclex-rn", Status = ExamStatus.Published };

    private static ExamVersion CreateExamVersion(Guid examId, ExamVersionStatus status) => new() { Id = Guid.NewGuid(), ExamId = examId, VersionNumber = 1, Status = status };

    private static ExamQuestion CreateQuestion(Guid versionId) => new() { Id = Guid.NewGuid(), ExamVersionId = versionId, QuestionText = "Protected question text", Points = 1, DisplayOrder = 1, IsActive = true };

    private static (Country Country, ExamCategory Category, PreparationPackageDefinition Definition, Exam Exam, ExamVersion ExamVersion, ReportingProfilePublication Profile, PracticeCollectionVersion PracticeVersion, ReportingTopic Topic, StudyMaterialVersion? MaterialVersion, List<ExamQuestion> Questions) CreateValidPackageGraph(bool includeMaterial = true)
    {
        var country = CreateCountry();
        var category = CreateCategory(countryId: country.Id);
        var definition = PreparationPackageDefinition.Create(country.Id, category.Id, "Package", "package", null);
        var exam = CreateExam(country.Id, category.Id);
        var examVersion = CreateExamVersion(exam.Id, ExamVersionStatus.Published);
        var question = CreateQuestion(examVersion.Id);
        var topic = ReportingTopic.Create(category.Id, "Topic", "topic", null);
        var profile = ReportingProfilePublication.CreateDraft(examVersion.Id, "Profile");
        profile.AssignQuestion(question.Id, topic.Id);
        profile.Publish(DateTime.UtcNow);
        var practiceVersion = PracticeCollectionVersion.CreateDraft(Guid.NewGuid(), 1);
        var practiceItem = PracticeItem.Create(topic.Id, "Practice prompt", "Practice feedback", 1);
        practiceItem.AddAnswerOption("A", true, 1);
        practiceItem.AddAnswerOption("B", false, 2);
        practiceVersion.AddPracticeItem(practiceItem);
        practiceVersion.Publish(DateTime.UtcNow);
        StudyMaterialVersion? materialVersion = null;
        if (includeMaterial)
        {
            materialVersion = StudyMaterialVersion.CreateDraft(Guid.NewGuid(), StudyMaterialType.FormattedText, "Study content", null, null, null, [topic.Id]);
            materialVersion.Publish(DateTime.UtcNow);
        }

        return (country, category, definition, exam, examVersion, profile, practiceVersion, topic, materialVersion, [question]);
    }
}
