using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PracticeCollectionVersion : AuditableEntity
{
    private readonly List<PracticeItem> _items = [];

    public Guid Id { get; private set; }
    public Guid PracticeCollectionId { get; private set; }
    public int VersionNumber { get; private set; }
    public PublicationStatus Status { get; private set; } = PublicationStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }
    public IReadOnlyCollection<PracticeItem> Items => _items.AsReadOnly();

    public static PracticeCollectionVersion CreateDraft(Guid practiceCollectionId, int versionNumber)
    {
        if (practiceCollectionId == Guid.Empty)
        {
            throw new InvalidOperationException("practiceCollectionId is required.");
        }

        return new PracticeCollectionVersion
        {
            Id = Guid.NewGuid(),
            PracticeCollectionId = practiceCollectionId,
            VersionNumber = versionNumber
        };
    }

    public void AddPracticeItem(PracticeItem item)
    {
        EnsureDraft();

        if (_items.Any(existing => existing.DisplayOrder == item.DisplayOrder))
        {
            throw new InvalidOperationException("Practice item display order must be unique within a collection version.");
        }

        item.SetPracticeCollectionVersionId(Id);
        _items.Add(item);
    }

    public void Publish(DateTime publishedAt)
    {
        EnsureDraft();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("A practice collection version must include at least one practice item before publication.");
        }

        if (_items.Any(item => !item.HasCorrectAnswerOption))
        {
            throw new InvalidOperationException("Every practice item must include a correct answer option before collection publication.");
        }

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
    }

    public void Retire(DateTime retiredAt)
    {
        if (Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException("Only published practice collection versions can be retired.");
        }

        Status = PublicationStatus.Retired;
        RetiredAt = retiredAt;
    }

    private void EnsureDraft()
    {
        if (Status != PublicationStatus.Draft)
        {
            throw new InvalidOperationException("Published or retired practice collection versions are immutable.");
        }
    }
}
