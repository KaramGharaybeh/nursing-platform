using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Employers;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Country> Countries { get; }
    DbSet<Language> Languages { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<NurseProfile> NurseProfiles { get; }
    DbSet<NurseExperience> NurseExperiences { get; }
    DbSet<NurseEducation> NurseEducation { get; }
    DbSet<NurseCertificate> NurseCertificates { get; }
    DbSet<NurseLanguage> NurseLanguages { get; }
    DbSet<NurseSkill> NurseSkills { get; }
    DbSet<NurseCvDocument> NurseCvDocuments { get; }
    DbSet<EmployerProfile> EmployerProfiles { get; }
    DbSet<EmployerOrganization> EmployerOrganizations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
