using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.PreparationPackages;

public class PracticeAnswerOption : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PracticeItemId { get; internal set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int DisplayOrder { get; private set; }

    internal static PracticeAnswerOption Create(string optionText, bool isCorrect, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(optionText))
        {
            throw new InvalidOperationException("Practice answer option text is required.");
        }

        if (displayOrder < 1)
        {
            throw new InvalidOperationException("Practice answer option display order must be positive.");
        }

        return new PracticeAnswerOption
        {
            Id = Guid.NewGuid(),
            OptionText = optionText.Trim(),
            IsCorrect = isCorrect,
            DisplayOrder = displayOrder
        };
    }
}
