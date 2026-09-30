using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PracticeItem : AuditableEntity
{
    private readonly List<PracticeAnswerOption> _answerOptions = [];

    public Guid Id { get; private set; }
    public Guid PracticeCollectionVersionId { get; private set; }
    public Guid ReportingTopicId { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string ImmediateFeedback { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public IReadOnlyCollection<PracticeAnswerOption> AnswerOptions => _answerOptions.AsReadOnly();
    internal bool HasCorrectAnswerOption => _answerOptions.Any(option => option.IsCorrect);

    public static PracticeItem Create(
        Guid reportingTopicId,
        string prompt,
        string immediateFeedback,
        int displayOrder)
    {
        if (reportingTopicId == Guid.Empty)
        {
            throw new InvalidOperationException("reportingTopicId is required.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidOperationException("Practice item prompt is required.");
        }

        if (string.IsNullOrWhiteSpace(immediateFeedback))
        {
            throw new InvalidOperationException("Practice item immediate feedback is required.");
        }

        if (displayOrder < 1)
        {
            throw new InvalidOperationException("Practice item display order must be positive.");
        }

        return new PracticeItem
        {
            Id = Guid.NewGuid(),
            ReportingTopicId = reportingTopicId,
            Prompt = prompt.Trim(),
            ImmediateFeedback = immediateFeedback.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void AddAnswerOption(string optionText, bool isCorrect, int displayOrder)
    {
        if (_answerOptions.Any(option => option.DisplayOrder == displayOrder))
        {
            throw new InvalidOperationException("Practice answer option display order must be unique within an item.");
        }

        var option = PracticeAnswerOption.Create(optionText, isCorrect, displayOrder);
        option.PracticeItemId = Id;
        _answerOptions.Add(option);
    }

    internal void SetPracticeCollectionVersionId(Guid practiceCollectionVersionId)
    {
        PracticeCollectionVersionId = practiceCollectionVersionId;
    }
}
