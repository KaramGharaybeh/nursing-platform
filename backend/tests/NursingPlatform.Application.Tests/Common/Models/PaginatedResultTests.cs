using NursingPlatform.Application.Common.Models;

namespace NursingPlatform.Application.Tests.Common.Models;

public class PaginatedResultTests
{
    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 25
        };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldNotRoundUp()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 20
        };

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithZeroItems_ShouldBeZero()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithPageSizeZero_ShouldReturnZero()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 0,
            TotalCount = 25
        };

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithPageSizeNegative_ShouldReturnZero()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = -1,
            TotalCount = 25
        };

        Assert.Equal(0, result.TotalPages);
    }
}
