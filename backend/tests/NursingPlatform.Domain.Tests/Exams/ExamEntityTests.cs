using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Domain.Tests.Exams;

public class ExamEntityTests
{
    [Fact]
    public void Exam_DefaultStatus_IsDraft()
    {
        var exam = new Exam();

        Assert.Equal(ExamStatus.Draft, exam.Status);
    }

    [Fact]
    public void ExamVersion_DefaultStatus_IsDraft()
    {
        var version = new ExamVersion();

        Assert.Equal(ExamVersionStatus.Draft, version.Status);
    }

    [Fact]
    public void ExamSession_DefaultStatus_IsInProgress()
    {
        var session = new ExamSession();

        Assert.Equal(ExamSessionStatus.InProgress, session.Status);
    }

    [Theory]
    [InlineData(ExamSessionStatus.InProgress, false)]
    [InlineData(ExamSessionStatus.Submitted, true)]
    [InlineData(ExamSessionStatus.Expired, true)]
    [InlineData(ExamSessionStatus.Abandoned, true)]
    public void ExamSession_TerminalStatusHelpers_IdentifySubmittedExpiredAndAbandoned(
        ExamSessionStatus status,
        bool expectedTerminal)
    {
        var session = new ExamSession { Status = status };

        Assert.Equal(expectedTerminal, session.IsTerminal);
    }

    [Fact]
    public void ExamSession_CalculatesExpiresAtFromStartedAtAndDuration()
    {
        var startedAt = new DateTime(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc);

        var session = ExamSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            startedAt,
            45);

        Assert.Equal(startedAt.AddMinutes(45), session.ExpiresAt);
    }

    [Fact]
    public void ExamQuestion_SupportsOnlySingleBestAnswerInPhase7A()
    {
        var question = new ExamQuestion();

        Assert.Equal(ExamQuestionType.SingleBestAnswer, question.QuestionType);
    }
}
