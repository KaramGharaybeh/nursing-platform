using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.ReferenceData.Countries.DTOs;
using NursingPlatform.Application.ReferenceData.Countries.Queries.ListCountries;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.ReferenceData.Countries;

public class ListCountriesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task ListCountries_ReturnsOnlyActiveCountriesSortedByName()
    {
        var active = new Country { Id = Guid.NewGuid(), Name = "Saudi Arabia", Code = "SA", IsActive = true };
        var anotherActive = new Country { Id = Guid.NewGuid(), Name = "Canada", Code = "CA", IsActive = true };
        var inactive = new Country { Id = Guid.NewGuid(), Name = "Aland Islands", Code = "AX", IsActive = false };
        ConfigureContext([active, anotherActive, inactive]);
        var handler = new ListCountriesQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListCountriesQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first => Assert.Equal("Canada", first.Name),
            second => Assert.Equal("Saudi Arabia", second.Name));
    }

    [Fact]
    public async Task ListCountries_ExposesOnlyIdNameAndCode()
    {
        var country = new Country { Id = Guid.NewGuid(), Name = "United States", Code = "US", IsActive = true };
        ConfigureContext([country]);
        var handler = new ListCountriesQueryHandler(_contextMock.Object);

        var result = await handler.Handle(new ListCountriesQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(country.Id, item.Id);
        Assert.Equal("United States", item.Name);
        Assert.Equal("US", item.Code);
        Assert.Null(typeof(CountryListItemDto).GetProperty("IsActive"));
    }

    private void ConfigureContext(IReadOnlyCollection<Country> countries)
    {
        _contextMock.Setup(c => c.Countries).Returns(countries.AsQueryable().BuildMockDbSet().Object);
    }
}
