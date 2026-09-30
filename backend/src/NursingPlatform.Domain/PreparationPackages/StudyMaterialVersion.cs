using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class StudyMaterialVersion : AuditableEntity
{
    private readonly List<StudyMaterialVersionTopic> _topics = [];

    public Guid Id { get; private set; }
    public Guid StudyMaterialId { get; private set; }
    public int VersionNumber { get; private set; }
    public StudyMaterialType MaterialType { get; private set; }
    public PublicationStatus Status { get; private set; } = PublicationStatus.Draft;
    public string? FormattedTextContent { get; private set; }
    public string? FileStorageKey { get; private set; }
    public string? ExternalUrl { get; private set; }
    public string? VideoUrl { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }
    public IReadOnlyCollection<StudyMaterialVersionTopic> Topics => _topics.AsReadOnly();

    public static StudyMaterialVersion CreateDraft(
        Guid studyMaterialId,
        StudyMaterialType materialType,
        string? formattedTextContent,
        string? fileStorageKey,
        string? externalUrl,
        string? videoUrl,
        IEnumerable<Guid> reportingTopicIds,
        int versionNumber = 1)
    {
        EnsureNotEmpty(studyMaterialId, nameof(studyMaterialId));

        var version = new StudyMaterialVersion
        {
            Id = Guid.NewGuid(),
            StudyMaterialId = studyMaterialId,
            VersionNumber = versionNumber
        };

        version.UpdateDraftContent(materialType, formattedTextContent, fileStorageKey, externalUrl, videoUrl, reportingTopicIds);
        return version;
    }

    public void UpdateDraftContent(
        StudyMaterialType materialType,
        string? formattedTextContent,
        string? fileStorageKey,
        string? externalUrl,
        string? videoUrl,
        IEnumerable<Guid> reportingTopicIds)
    {
        EnsureDraft();
        EnsureMaterialContentMatchesType(materialType, formattedTextContent, fileStorageKey, externalUrl, videoUrl);

        var topicIds = reportingTopicIds.Distinct().ToList();
        if (topicIds.Count == 0)
        {
            throw new InvalidOperationException("A material version must map to at least one reporting topic.");
        }

        MaterialType = materialType;
        FormattedTextContent = NormalizeOptional(formattedTextContent);
        FileStorageKey = NormalizeOptional(fileStorageKey);
        ExternalUrl = NormalizeOptional(externalUrl);
        VideoUrl = NormalizeOptional(videoUrl);

        _topics.Clear();
        foreach (var topicId in topicIds)
        {
            var topic = StudyMaterialVersionTopic.Create(topicId);
            topic.StudyMaterialVersionId = Id;
            _topics.Add(topic);
        }
    }

    public void Publish(DateTime publishedAt)
    {
        EnsureDraft();

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
    }

    public void Retire(DateTime retiredAt)
    {
        if (Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException("Only published material versions can be retired.");
        }

        Status = PublicationStatus.Retired;
        RetiredAt = retiredAt;
    }

    private void EnsureDraft()
    {
        if (Status != PublicationStatus.Draft)
        {
            throw new InvalidOperationException("Published or retired material versions are immutable.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void EnsureMaterialContentMatchesType(
        StudyMaterialType materialType,
        string? formattedTextContent,
        string? fileStorageKey,
        string? externalUrl,
        string? videoUrl)
    {
        var hasFormattedText = !string.IsNullOrWhiteSpace(formattedTextContent);
        var hasFileStorageKey = !string.IsNullOrWhiteSpace(fileStorageKey);
        var hasExternalUrl = !string.IsNullOrWhiteSpace(externalUrl);
        var hasVideoUrl = !string.IsNullOrWhiteSpace(videoUrl);

        var valid = materialType switch
        {
            StudyMaterialType.FormattedText => hasFormattedText && !hasFileStorageKey && !hasExternalUrl && !hasVideoUrl,
            StudyMaterialType.File => hasFileStorageKey && !hasFormattedText && !hasExternalUrl && !hasVideoUrl,
            StudyMaterialType.ExternalLink => hasExternalUrl && !hasFormattedText && !hasFileStorageKey && !hasVideoUrl,
            StudyMaterialType.Video => hasVideoUrl && !hasFormattedText && !hasFileStorageKey && !hasExternalUrl,
            _ => false
        };

        if (!valid)
        {
            throw new InvalidOperationException("Material content must match the selected material type.");
        }
    }

    private static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationException($"{parameterName} is required.");
        }
    }
}
