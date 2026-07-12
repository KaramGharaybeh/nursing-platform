using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Exams;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class ExamCategoryConfiguration : IEntityTypeConfiguration<ExamCategory>
{
    public void Configure(EntityTypeBuilder<ExamCategory> builder)
    {
        builder.ToTable("ExamCategories");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.CountryId, c.Slug }).IsUnique();
        builder.HasIndex(c => new { c.CountryId, c.DisplayOrder, c.Id });
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => new { e.Status, e.CountryId, e.ExamCategoryId, e.Title, e.Id });
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(160);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Instructions).HasMaxLength(4000);
        builder.Property(e => e.PassingScorePercentage).HasPrecision(5, 2);
        builder.Property(e => e.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ExamCategory)
            .WithMany()
            .HasForeignKey(e => e.ExamCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamVersionConfiguration : IEntityTypeConfiguration<ExamVersion>
{
    public void Configure(EntityTypeBuilder<ExamVersion> builder)
    {
        builder.ToTable("ExamVersions");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => new { v.ExamId, v.VersionNumber }).IsUnique();
        builder.HasIndex(v => new { v.ExamId, v.Status, v.VersionNumber });
        builder.Property(v => v.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne(v => v.Exam)
            .WithMany()
            .HasForeignKey(v => v.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
{
    public void Configure(EntityTypeBuilder<ExamQuestion> builder)
    {
        builder.ToTable("ExamQuestions");
        builder.HasKey(q => q.Id);
        builder.HasIndex(q => new { q.ExamVersionId, q.DisplayOrder, q.Id });
        builder.Property(q => q.QuestionText).IsRequired().HasMaxLength(4000);
        builder.Property(q => q.Explanation).HasMaxLength(4000);
        builder.Property(q => q.QuestionType).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.HasOne(q => q.ExamVersion)
            .WithMany()
            .HasForeignKey(q => q.ExamVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamAnswerOptionConfiguration : IEntityTypeConfiguration<ExamAnswerOption>
{
    public void Configure(EntityTypeBuilder<ExamAnswerOption> builder)
    {
        builder.ToTable("ExamAnswerOptions");
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.ExamQuestionId, o.DisplayOrder, o.Id });
        builder.Property(o => o.OptionText).IsRequired().HasMaxLength(2000);
        builder.HasOne(o => o.ExamQuestion)
            .WithMany()
            .HasForeignKey(o => o.ExamQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamAccessGrantConfiguration : IEntityTypeConfiguration<ExamAccessGrant>
{
    public void Configure(EntityTypeBuilder<ExamAccessGrant> builder)
    {
        builder.ToTable("ExamAccessGrants");
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => new { g.NurseProfileId, g.ExamId, g.ExpiresAt });
        builder.Property(g => g.Reason).HasMaxLength(500);
        builder.HasOne(g => g.NurseProfile)
            .WithMany()
            .HasForeignKey(g => g.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.Exam)
            .WithMany()
            .HasForeignKey(g => g.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
{
    public void Configure(EntityTypeBuilder<ExamSession> builder)
    {
        builder.ToTable("ExamSessions");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.NurseProfileId, s.StartedAt, s.Id });
        builder.HasIndex(s => new { s.NurseProfileId, s.ExamVersionId })
            .IsUnique()
            .HasFilter("\"Status\" = 'InProgress'");
        builder.Property(s => s.Status).HasConversion<string>().IsRequired().HasMaxLength(32);
        builder.Property(s => s.Percentage).HasPrecision(5, 2);
        builder.HasOne(s => s.NurseProfile)
            .WithMany()
            .HasForeignKey(s => s.NurseProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Exam)
            .WithMany()
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ExamVersion)
            .WithMany()
            .HasForeignKey(s => s.ExamVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamSessionQuestionConfiguration : IEntityTypeConfiguration<ExamSessionQuestion>
{
    public void Configure(EntityTypeBuilder<ExamSessionQuestion> builder)
    {
        builder.ToTable("ExamSessionQuestions");
        builder.HasKey(q => q.Id);
        builder.HasIndex(q => new { q.ExamSessionId, q.DisplayOrder, q.Id });
        builder.Property(q => q.QuestionTextSnapshot).IsRequired().HasMaxLength(4000);
        builder.Property(q => q.ExplanationSnapshot).HasMaxLength(4000);
        builder.HasOne(q => q.ExamSession)
            .WithMany()
            .HasForeignKey(q => q.ExamSessionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.ExamQuestion)
            .WithMany()
            .HasForeignKey(q => q.ExamQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamSessionAnswerOptionConfiguration : IEntityTypeConfiguration<ExamSessionAnswerOption>
{
    public void Configure(EntityTypeBuilder<ExamSessionAnswerOption> builder)
    {
        builder.ToTable("ExamSessionAnswerOptions");
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.ExamSessionQuestionId, o.DisplayOrder, o.Id });
        builder.Property(o => o.OptionTextSnapshot).IsRequired().HasMaxLength(2000);
        builder.HasOne(o => o.ExamSessionQuestion)
            .WithMany()
            .HasForeignKey(o => o.ExamSessionQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.ExamAnswerOption)
            .WithMany()
            .HasForeignKey(o => o.ExamAnswerOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamSessionAnswerConfiguration : IEntityTypeConfiguration<ExamSessionAnswer>
{
    public void Configure(EntityTypeBuilder<ExamSessionAnswer> builder)
    {
        builder.ToTable("ExamSessionAnswers");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.ExamSessionQuestionId).IsUnique();
        builder.HasOne(a => a.ExamSessionQuestion)
            .WithMany()
            .HasForeignKey(a => a.ExamSessionQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.SelectedExamSessionAnswerOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedExamSessionAnswerOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
