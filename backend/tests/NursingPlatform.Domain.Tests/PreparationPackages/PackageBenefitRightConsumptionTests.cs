using NursingPlatform.Domain.PreparationPackages;

namespace NursingPlatform.Domain.Tests.PreparationPackages;

public class PackageBenefitRightConsumptionTests
{
    [Fact]
    public void PackageBenefitRight_ConsumePackageExamAttempt_TransitionsAvailableToConsumed()
    {
        var right = CreateAttemptRight(PackageBenefitRightStatus.Available);

        right.ConsumePackageExamAttempt(new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal(PackageBenefitRightStatus.Consumed, right.Status);
    }

    [Fact]
    public void PackageBenefitRight_ConsumePackageExamAttempt_SetsConsumedAt()
    {
        var consumedAt = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var right = CreateAttemptRight(PackageBenefitRightStatus.Available);

        right.ConsumePackageExamAttempt(consumedAt);

        Assert.Equal(consumedAt, right.ConsumedAt);
    }

    [Theory]
    [InlineData(PackageBenefitRightStatus.Consumed)]
    [InlineData(PackageBenefitRightStatus.Dormant)]
    [InlineData(PackageBenefitRightStatus.Expired)]
    [InlineData(PackageBenefitRightStatus.Revoked)]
    public void PackageBenefitRight_ConsumePackageExamAttempt_WhenNotAvailable_Throws(PackageBenefitRightStatus status)
    {
        var right = CreateAttemptRight(status);

        Assert.Throws<InvalidOperationException>(() => right.ConsumePackageExamAttempt(new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageBenefitRight_ConsumePackageExamAttempt_WhenBeforeAccessWindow_Throws()
    {
        var right = CreateAttemptRight(PackageBenefitRightStatus.Available);

        Assert.Throws<InvalidOperationException>(() => right.ConsumePackageExamAttempt(new DateTime(2026, 7, 29, 11, 59, 59, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageBenefitRight_ConsumePackageExamAttempt_WhenAfterAccessWindow_Throws()
    {
        var right = CreateAttemptRight(PackageBenefitRightStatus.Available);

        Assert.Throws<InvalidOperationException>(() => right.ConsumePackageExamAttempt(new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageBenefitRight_ConsumePackageExamAttempt_WhenWrongRightType_Throws()
    {
        var right = PackageBenefitRight.Create(
            Guid.NewGuid(),
            PackageBenefitRightType.MaterialsAccess,
            PackageBenefitRightStatus.Available,
            new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc));

        Assert.Throws<InvalidOperationException>(() => right.ConsumePackageExamAttempt(new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void PackageBenefitRight_Expire_DoesNotOverwriteConsumedAttemptRight()
    {
        var consumedAt = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var right = CreateAttemptRight(PackageBenefitRightStatus.Available);

        right.ConsumePackageExamAttempt(consumedAt);
        right.Expire(consumedAt.AddDays(90));

        Assert.Equal(PackageBenefitRightStatus.Consumed, right.Status);
        Assert.Equal(consumedAt, right.ConsumedAt);
    }

    private static PackageBenefitRight CreateAttemptRight(PackageBenefitRightStatus status)
    {
        return PackageBenefitRight.Create(
            Guid.NewGuid(),
            PackageBenefitRightType.PackageExamAttemptEligibility,
            status,
            new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc));
    }
}
