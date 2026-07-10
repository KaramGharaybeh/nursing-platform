using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Nurses.Common;
using NursingPlatform.Application.Nurses.DTOs;
using NursingPlatform.Domain.Nurses;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Nurses.Commands.UpdateNurseLanguages;

public class UpdateNurseLanguagesCommandHandler : IRequestHandler<UpdateNurseLanguagesCommand, IReadOnlyList<NurseLanguageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly NurseRoleGuard _nurseRoleGuard;

    public UpdateNurseLanguagesCommandHandler(IApplicationDbContext context, NurseRoleGuard nurseRoleGuard)
    {
        _context = context;
        _nurseRoleGuard = nurseRoleGuard;
    }

    public async Task<IReadOnlyList<NurseLanguageDto>> Handle(UpdateNurseLanguagesCommand command, CancellationToken cancellationToken)
    {
        var profile = await GetCurrentProfileAsync(cancellationToken);
        var requestedLanguageIds = command.Languages.Select(l => l.LanguageId).Distinct().ToList();
        var languages = await GetActiveLanguagesAsync(requestedLanguageIds, cancellationToken);

        var existingLanguages = await _context.NurseLanguages
            .Where(l => l.NurseProfileId == profile.Id)
            .ToListAsync(cancellationToken);

        foreach (var existingLanguage in existingLanguages)
        {
            _context.NurseLanguages.Remove(existingLanguage);
        }

        var createdLanguages = command.Languages
            .Select(request => new NurseLanguage
            {
                Id = Guid.NewGuid(),
                NurseProfileId = profile.Id,
                LanguageId = request.LanguageId,
                Proficiency = request.Proficiency
            })
            .ToList();

        foreach (var language in createdLanguages)
        {
            _context.NurseLanguages.Add(language);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdLanguages
            .Select(language => ToDto(language, languages[language.LanguageId]))
            .OrderBy(language => language.Name)
            .ToList();
    }

    private async Task<NurseProfile> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        var userId = await _nurseRoleGuard.EnsureCurrentUserIsNurseAsync(cancellationToken);
        var profile = await _context.NurseProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException("Nurse profile was not found.");
        }

        return profile;
    }

    private async Task<Dictionary<Guid, Language>> GetActiveLanguagesAsync(
        IReadOnlyCollection<Guid> languageIds,
        CancellationToken cancellationToken)
    {
        if (languageIds.Count == 0)
        {
            return [];
        }

        var languages = await _context.Languages
            .Where(l => languageIds.Contains(l.Id) && l.IsActive)
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        if (languages.Count != languageIds.Count)
        {
            throw new InvalidOperationException("One or more languages are invalid or inactive.");
        }

        return languages;
    }

    private static NurseLanguageDto ToDto(NurseLanguage nurseLanguage, Language language)
    {
        return new NurseLanguageDto
        {
            Id = nurseLanguage.Id,
            LanguageId = nurseLanguage.LanguageId,
            Name = language.Name,
            Code = language.Code,
            Proficiency = nurseLanguage.Proficiency
        };
    }
}
