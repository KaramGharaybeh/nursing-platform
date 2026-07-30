using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Application.PreparationPackages.Reports.Generation;

internal static class PackageAnalyticalReportGenerator
{
    public static async Task<PackageAnalyticalReport> GenerateAsync(
        IApplicationDbContext context,
        ExamSession session,
        ExamSessionProvenance provenance,
        PackagePurchaseEntitlement entitlement,
        DateTime generatedAt,
        CancellationToken cancellationToken)
    {
        if (session.FinalizedAt is null)
        {
            throw Conflict("package-report-session-not-finalized", "Package exam session has not been finalized.");
        }

        var questions = await context.ExamSessionQuestions
            .AsNoTracking()
            .Where(question => question.ExamSessionId == session.Id)
            .OrderBy(question => question.DisplayOrder)
            .ThenBy(question => question.Id)
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(question => question.Id).ToList();
        var sourceQuestionIds = questions.Select(question => question.ExamQuestionId).ToList();

        var options = await context.ExamSessionAnswerOptions
            .AsNoTracking()
            .Where(option => questionIds.Contains(option.ExamSessionQuestionId))
            .ToListAsync(cancellationToken);

        var answers = await context.ExamSessionAnswers
            .AsNoTracking()
            .Where(answer => questionIds.Contains(answer.ExamSessionQuestionId))
            .ToListAsync(cancellationToken);

        var assignments = await context.ReportingProfileQuestionAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ReportingProfilePublicationId == provenance.ReportingProfilePublicationId
                && sourceQuestionIds.Contains(assignment.ExamQuestionId))
            .ToListAsync(cancellationToken);

        if (questions.Count == 0 || assignments.Select(assignment => assignment.ExamQuestionId).Distinct().Count() != sourceQuestionIds.Distinct().Count())
        {
            throw Conflict("package-report-profile-incomplete", "Package reporting profile does not cover finalized session evidence.");
        }

        var topicIds = assignments.Select(assignment => assignment.ReportingTopicId).Distinct().ToList();
        var topics = await context.ReportingTopics
            .AsNoTracking()
            .Where(topic => topicIds.Contains(topic.Id))
            .ToDictionaryAsync(topic => topic.Id, cancellationToken);

        if (topics.Count != topicIds.Count)
        {
            throw Conflict("package-report-profile-incomplete", "Package reporting topics are incomplete.");
        }

        var report = PackageAnalyticalReport.Create(
            session.NurseProfileId,
            session.Id,
            provenance.Id,
            entitlement.Id,
            provenance.PackageOrderItemSnapshotId,
            provenance.PaymentOrderId,
            provenance.PaymentOrderItemId,
            provenance.PreparationPackageDefinitionId,
            provenance.PreparationPackageVersionId,
            provenance.PreparationPackageOfferId,
            provenance.IncludedExamId,
            provenance.IncludedExamVersionId,
            provenance.ReportingProfilePublicationId,
            provenance.PracticeCollectionVersionId,
            generatedAt,
            session.Status,
            session.SubmittedAt,
            session.FinalizedAt.Value,
            session.Score,
            session.MaxScore,
            session.Percentage,
            session.Passed,
            session.CorrectCount,
            session.QuestionCount,
            provenance.PackageAccessStartsAt,
            provenance.PackageAccessEndsAt);

        AddTopicResults(report, questions, options, answers, assignments, topics);
        await AddGuidanceAsync(context, report, entitlement, provenance, cancellationToken);

        return report;
    }

    private static void AddTopicResults(
        PackageAnalyticalReport report,
        IReadOnlyCollection<ExamSessionQuestion> questions,
        IReadOnlyCollection<ExamSessionAnswerOption> options,
        IReadOnlyCollection<ExamSessionAnswer> answers,
        IReadOnlyCollection<ReportingProfileQuestionAssignment> assignments,
        IReadOnlyDictionary<Guid, ReportingTopic> topics)
    {
        var optionById = options.ToDictionary(option => option.Id);
        var selectedByQuestionId = answers.ToDictionary(answer => answer.ExamSessionQuestionId, answer => answer.SelectedExamSessionAnswerOptionId);
        var assignmentBySourceQuestionId = assignments.ToDictionary(assignment => assignment.ExamQuestionId);

        var rows = questions
            .Select(question => new TopicQuestionEvidence(
                Question: question,
                TopicId: assignmentBySourceQuestionId[question.ExamQuestionId].ReportingTopicId,
                IsCorrect: selectedByQuestionId.TryGetValue(question.Id, out var selectedOptionId)
                    && optionById.TryGetValue(selectedOptionId, out var selectedOption)
                    && selectedOption.IsCorrectSnapshot))
            .GroupBy(row => row.TopicId)
            .Select(group =>
            {
                var topic = topics[group.Key];
                var orderedQuestions = group.Select(row => row.Question).OrderBy(question => question.DisplayOrder).ThenBy(question => question.Id).ToList();
                var availablePoints = orderedQuestions.Sum(question => question.Points);
                var earnedPoints = group.Where(row => row.IsCorrect).Sum(row => row.Question.Points);
                var scoredQuestionCount = orderedQuestions.Count;
                var correctCount = group.Count(row => row.IsCorrect);
                var percentage = availablePoints == 0
                    ? 0m
                    : Math.Round(earnedPoints * 100m / availablePoints, 2, MidpointRounding.AwayFromZero);

                return new
                {
                    Topic = topic,
                    FirstQuestionOrder = orderedQuestions[0].DisplayOrder,
                    ScoredQuestionCount = scoredQuestionCount,
                    CorrectCount = correctCount,
                    EarnedPoints = earnedPoints,
                    AvailablePoints = availablePoints,
                    Percentage = percentage
                };
            })
            .OrderBy(row => row.FirstQuestionOrder)
            .ThenBy(row => row.Topic.Name)
            .ThenBy(row => row.Topic.Id)
            .ToList();

        var sortOrder = 1;
        foreach (var row in rows)
        {
            report.AddTopicResult(
                row.Topic.Id,
                row.Topic.Name,
                row.Topic.Description,
                row.ScoredQuestionCount,
                row.CorrectCount,
                row.EarnedPoints,
                row.AvailablePoints,
                row.Percentage,
                sortOrder++);
        }
    }

    private static async Task AddGuidanceAsync(
        IApplicationDbContext context,
        PackageAnalyticalReport report,
        PackagePurchaseEntitlement entitlement,
        ExamSessionProvenance provenance,
        CancellationToken cancellationToken)
    {
        var topicOrders = report.TopicResults.ToDictionary(topic => topic.ReportingTopicId, topic => topic.SortOrder);
        var topicIds = topicOrders.Keys.ToList();
        var purchasedMaterialIds = entitlement.StudyMaterialVersionIds.ToList();

        var materialRows = await (
            from version in context.StudyMaterialVersions.AsNoTracking()
            join material in context.StudyMaterials.AsNoTracking() on version.StudyMaterialId equals material.Id
            join topic in context.StudyMaterialVersionTopics.AsNoTracking() on version.Id equals topic.StudyMaterialVersionId
            where purchasedMaterialIds.Contains(version.Id) && topicIds.Contains(topic.ReportingTopicId)
            select new
            {
                version.Id,
                version.MaterialType,
                material.Title,
                topic.ReportingTopicId
            })
            .ToListAsync(cancellationToken);

        var practiceRows = await (
            from version in context.PracticeCollectionVersions.AsNoTracking()
            join collection in context.PracticeCollections.AsNoTracking() on version.PracticeCollectionId equals collection.Id
            join item in context.PracticeItems.AsNoTracking() on version.Id equals item.PracticeCollectionVersionId
            where version.Id == provenance.PracticeCollectionVersionId && topicIds.Contains(item.ReportingTopicId)
            select new
            {
                version.Id,
                collection.Title,
                item.ReportingTopicId
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var guidanceRows = materialRows
            .Select(row => new GuidanceRow(
                row.ReportingTopicId,
                PackageReportGuidanceSourceType.StudyMaterialVersion,
                row.Id,
                row.Title,
                row.MaterialType.ToString(),
                purchasedMaterialIds.IndexOf(row.Id)))
            .Concat(practiceRows.Select(row => new GuidanceRow(
                row.ReportingTopicId,
                PackageReportGuidanceSourceType.PracticeCollectionVersion,
                row.Id,
                row.Title,
                "PracticeCollectionVersion",
                int.MaxValue)))
            .OrderBy(row => topicOrders[row.ReportingTopicId])
            .ThenBy(row => row.SourceType)
            .ThenBy(row => row.PurchasedMaterialOrder)
            .ThenBy(row => row.Title)
            .ThenBy(row => row.SourceVersionId)
            .ToList();

        var sortOrder = report.TopicResults.Count + 1;
        foreach (var row in guidanceRows)
        {
            report.AddGuidanceItem(
                row.ReportingTopicId,
                row.SourceType,
                row.SourceVersionId,
                row.Title,
                row.SourceMetadata,
                sortOrder++);
        }
    }

    private static PackageReportConflictException Conflict(string code, string message)
    {
        return new PackageReportConflictException(code, message);
    }

    private sealed record TopicQuestionEvidence(ExamSessionQuestion Question, Guid TopicId, bool IsCorrect);

    private sealed record GuidanceRow(
        Guid ReportingTopicId,
        PackageReportGuidanceSourceType SourceType,
        Guid SourceVersionId,
        string Title,
        string SourceMetadata,
        int PurchasedMaterialOrder);
}
