using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.ReferenceData.Languages.DTOs;
using NursingPlatform.Application.ReferenceData.Languages.Queries.ListLanguages;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.ReferenceData.Languages;

public class ListLanguagesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListLanguages_ReturnsOnlyActiveLanguagesSortedByName()
    {
        var active = new Language { Id = Guid.NewGuid(), Name = "Spanish", Code = "ES", IsActive = true };
        var anotherActive = new Language { Id = Guid.NewGuid(), Name = "Arabic", Code = "AR", IsActive = true };
        var inactive = new Language { Id = Guid.NewGuid(), Name = "Afar", Code = "AA", IsActive = false };
        ConfigureContext([active, anotherActive, inactive]);
        var handler = new ListLanguagesQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListLanguagesQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal("Arabic", first.Name),
            second => Assert.Equal("Spanish", second.Name));
    }

    [Fact]
    public async Task ListLanguages_ExposesOnlyIdNameAndCode()
    {
        var language = new Language { Id = Guid.NewGuid(), Name = "English", Code = "EN", IsActive = true };
        ConfigureContext([language]);
        var handler = new ListLanguagesQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListLanguagesQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(language.Id, item.Id);
        Assert.Equal("English", item.Name);
        Assert.Equal("EN", item.Code);
        Assert.Null(typeof(LanguageListItemDto).GetProperty("IsActive"));
    }

    private void ConfigureContext(IReadOnlyCollection<Language> languages)
    {
        _contextMock.Setup(c => c.Languages).Returns(languages.AsQueryable().BuildMockDbSet().Object);
    }
}
