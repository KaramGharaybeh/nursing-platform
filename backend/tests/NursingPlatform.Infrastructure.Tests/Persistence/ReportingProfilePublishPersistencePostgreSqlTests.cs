using Microsoft.EntityFrameworkCore;
using Npgsql;
using NursingPlatform.Application.PreparationPackages.Admin.ReportingProfiles;
using NursingPlatform.Domain.Exams;
using NursingPlatform.Domain.PreparationPackages;
using NursingPlatform.Domain.ReferenceData;
using NursingPlatform.Infrastructure.Persistence;

namespace NursingPlatform.Infrastructure.Tests.Persistence;

public sealed class ReportingProfilePublishPersistencePostgreSqlTests : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING";
    private readonly string _databaseName = $"nps_profile_publish_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _maintenanceConnectionString;

    public ReportingProfilePublishPersistencePostgreSqlTests()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException($"Set {ConnectionStringEnvironmentVariable} to run PostgreSQL reporting profile publish tests.");
        }

        _connectionString = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = _databaseName }.ConnectionString;
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" }.ConnectionString;
    }

    [Fact]
    public async Task Publish_WithNewAssignmentOnPersistedDraft_InsertsAssignmentAndPublishes()
    {
        var seed = await SeedDraftProfileGraphAsync();

        await using var context = CreateContext();
        var handler = new PublishAdminReportingProfileCommandHandler(context);
        var result = await handler.Handle(new PublishAdminReportingProfileCommand
        {
            Id = seed.ProfileId,
            Request = new PublishAdminReportingProfileRequest
            {
                Assignments =
                [
                    new ReportingProfileQuestionAssignmentRequest
                    {
                        ExamQuestionId = seed.QuestionId,
                        ReportingTopicId = seed.TopicId
                    }
                ]
            }
        }, CancellationToken.None);

        Assert.Equal(PublicationStatus.Published.ToString(), result.Status);

        await using var verify = CreateContext();
        var profile = await verify.ReportingProfilePublications.AsNoTracking().SingleAsync(p => p.Id == seed.ProfileId);
        Assert.Equal(PublicationStatus.Published, profile.Status);
        var assignments = await verify.ReportingProfileQuestionAssignments.AsNoTracking()
            .Where(a => a.ReportingProfilePublicationId == seed.ProfileId)
            .ToListAsync();
        var assignment = Assert.Single(assignments);
        Assert.Equal(seed.QuestionId, assignment.ExamQuestionId);
        Assert.Equal(seed.TopicId, assignment.ReportingTopicId);
    }

    private async Task<(Guid ProfileId, Guid QuestionId, Guid TopicId)> SeedDraftProfileGraphAsync()
    {
        await using var context = CreateContext();
        var country = new Country { Id = Guid.NewGuid(), Name = $"Country {Guid.NewGuid():N}", Code = "QZ" };
        var category = new ExamCategory { Id = Guid.NewGuid(), CountryId = country.Id, Name = $"Category {Guid.NewGuid():N}", Slug = Guid.NewGuid().ToString("N"), DisplayOrder = 1 };
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CountryId = country.Id,
            ExamCategoryId = category.Id,
            Title = $"Exam {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            DurationMinutes = 60,
            PassingScorePercentage = 70,
            Status = ExamStatus.Published,
            IsFree = true
        };
        var version = new ExamVersion { Id = Guid.NewGuid(), ExamId = exam.Id, VersionNumber = 1, Status = ExamVersionStatus.Published };
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamVersionId = version.Id,
            DisplayOrder = 1,
            QuestionText = "Question?",
            QuestionType = ExamQuestionType.SingleBestAnswer,
            Points = 1,
            IsActive = true
        };
        var topic = ReportingTopic.Create(category.Id, $"Topic {Guid.NewGuid():N}", Guid.NewGuid().ToString("N"), null);
        var profile = ReportingProfilePublication.CreateDraft(version.Id, $"Profile {Guid.NewGuid():N}");
        context.Countries.Add(country);
        context.ExamCategories.Add(category);
        context.Exams.Add(exam);
        context.ExamVersions.Add(version);
        context.ExamQuestions.Add(question);
        context.ReportingTopics.Add(topic);
        context.ReportingProfilePublications.Add(profile);
        await context.SaveChangesAsync();

        return (profile.Id, question.Id, topic.Id);
    }

    public async Task InitializeAsync()
    {
        await using (var connection = new NpgsqlConnection(_maintenanceConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using (var terminateCommand = connection.CreateCommand())
        {
            terminateCommand.CommandText = $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid();";
            await terminateCommand.ExecuteNonQueryAsync();
        }

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE \"{_databaseName}\"";
        await dropCommand.ExecuteNonQueryAsync();
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }
}
