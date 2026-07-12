namespace NursingPlatform.Application.Exams.Analytics.Common;

public class ExamAnalyticsFilters
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ExamId { get; set; }
}
