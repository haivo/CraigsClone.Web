using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.Services;

// The pure parts of query building. No database: ApplySort runs against an in-memory list.
[Trait("Category", "Unit")]
public class ListingServiceHelpersTests
{
    [Theory]
    [InlineData("50%", @"50\%")]
    [InlineData("a_b", @"a\_b")]
    [InlineData(@"a\b", @"a\\b")]
    [InlineData("plain", "plain")]
    [InlineData("100%_done", @"100\%\_done")]
    public void EscapeLike_EscapesWildcardsAndBackslash(string input, string expected)
    {
        Assert.Equal(expected, ListingService.EscapeLike(input));
    }

    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static List<Listing> Sample() =>
    [
        new() { Title = "cheap-old", Price = 10, CreatedAt = T0 },
        new() { Title = "dear", Price = 100, CreatedAt = T0.AddHours(1) },
        new() { Title = "cheap-new", Price = 10, CreatedAt = T0.AddHours(2) },
        new() { Title = "unpriced", Price = null, CreatedAt = T0.AddHours(3) },
    ];

    private static List<string> Titles(ListingSort sort) =>
        ListingService.ApplySort(Sample().AsQueryable(), sort).Select(l => l.Title).ToList();

    [Fact]
    public void Newest_IsCreatedAtDescending()
    {
        Assert.Equal(["unpriced", "cheap-new", "dear", "cheap-old"], Titles(ListingSort.Newest));
    }

    [Fact]
    public void PriceAsc_CheapestFirst_TiesNewestFirst_UnpricedLast()
    {
        Assert.Equal(["cheap-new", "cheap-old", "dear", "unpriced"], Titles(ListingSort.PriceAsc));
    }

    [Fact]
    public void PriceDesc_DearestFirst_TiesNewestFirst_UnpricedLast()
    {
        Assert.Equal(["dear", "cheap-new", "cheap-old", "unpriced"], Titles(ListingSort.PriceDesc));
    }
}
