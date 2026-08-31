using Contoso.CustomerApi.Models;
using Contoso.CustomerApi.Repositories;
using Contoso.CustomerApi.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Contoso.CustomerApi.Tests;

/// <summary>
/// Pagination boundary tests.
///
/// These are the tests Demo 03 breaks. The seed set has 23 customers, so a page size
/// of 10 gives three pages and a partial last page - the shape that exposes an
/// off-by-one in the skip calculation.
/// </summary>
public class PaginationTests
{
    private static CustomerService CreateService() =>
        new(new InMemoryCustomerRepository(), NullLogger<CustomerService>.Instance);

    private const int SeedCount = 23;

    [Fact]
    public async Task FirstPage_ReturnsFirstItems_NotSkippedOnes()
    {
        var service = CreateService();
        var all = await new InMemoryCustomerRepository().GetAllAsync();

        var result = await service.GetCustomersAsync(page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        var paged = result.Value!;

        Assert.Equal(10, paged.Items.Count);
        Assert.Equal(all[0].Id, paged.Items[0].Id);
        Assert.Equal(all[9].Id, paged.Items[9].Id);
    }

    [Fact]
    public async Task SecondPage_ContinuesExactlyWhereFirstPageEnded()
    {
        var service = CreateService();

        var first = await service.GetCustomersAsync(page: 1, pageSize: 10);
        var second = await service.GetCustomersAsync(page: 2, pageSize: 10);

        var firstIds = first.Value!.Items.Select(c => c.Id).ToList();
        var secondIds = second.Value!.Items.Select(c => c.Id).ToList();

        Assert.Equal(10, secondIds.Count);
        Assert.Empty(firstIds.Intersect(secondIds));
    }

    [Fact]
    public async Task LastPage_ReturnsRemainder()
    {
        var service = CreateService();

        var result = await service.GetCustomersAsync(page: 3, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(SeedCount - 20, result.Value!.Items.Count);
    }

    [Fact]
    public async Task EveryPage_TogetherCoversTheWholeSetExactlyOnce()
    {
        var service = CreateService();
        var seen = new List<Guid>();

        for (var page = 1; page <= 3; page++)
        {
            var result = await service.GetCustomersAsync(page, pageSize: 10);
            seen.AddRange(result.Value!.Items.Select(c => c.Id));
        }

        Assert.Equal(SeedCount, seen.Count);
        Assert.Equal(SeedCount, seen.Distinct().Count());
    }

    [Fact]
    public async Task PageBeyondTheEnd_ReturnsEmptyButStillReportsTotals()
    {
        var service = CreateService();

        var result = await service.GetCustomersAsync(page: 99, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(SeedCount, result.Value.TotalCount);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, 10, 1, 10)]
    [InlineData(1, 0, 1, 10)]
    [InlineData(1, 5000, 1, 100)]
    [InlineData(null, null, 1, 10)]
    public void Normalize_ClampsOutOfRangeValues(int? page, int? pageSize, int expectedPage, int expectedSize)
    {
        var (p, s) = PagingDefaults.Normalize(page, pageSize);

        Assert.Equal(expectedPage, p);
        Assert.Equal(expectedSize, s);
    }

    [Fact]
    public async Task Metadata_ReportsNavigationFlagsCorrectly()
    {
        var service = CreateService();

        var first = (await service.GetCustomersAsync(1, 10)).Value!;
        var middle = (await service.GetCustomersAsync(2, 10)).Value!;
        var last = (await service.GetCustomersAsync(3, 10)).Value!;

        Assert.Equal(3, first.TotalPages);

        Assert.False(first.HasPreviousPage);
        Assert.True(first.HasNextPage);

        Assert.True(middle.HasPreviousPage);
        Assert.True(middle.HasNextPage);

        Assert.True(last.HasPreviousPage);
        Assert.False(last.HasNextPage);
    }

    [Fact]
    public async Task CountryFilter_PagesTheFilteredSetNotTheWholeSet()
    {
        var service = CreateService();

        var result = await service.GetCustomersAsync(page: 1, pageSize: 10, country: "TR");

        Assert.True(result.IsSuccess);
        Assert.All(result.Value!.Items, c => Assert.Equal("TR", c.Country));
        Assert.Equal(result.Value.Items.Count, result.Value.TotalCount);
    }
}
