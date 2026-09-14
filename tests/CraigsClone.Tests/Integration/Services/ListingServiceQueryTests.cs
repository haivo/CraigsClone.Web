using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Integration.Services;

// Keyword (ILIKE), price range, sort and paging against real Postgres.
// Every test owns a fresh city + category, so its rows are isolated from every other test's.
[Trait("Category", "Integration")]
[Collection("postgres")]
public class ListingServiceQueryTests(PostgresFixture fixture)
{
    private sealed class Scope(AppDbContext db, City city, Category category)
    {
        public AppDbContext Db { get; } = db;
        public ListingService Service { get; } = new(db);
        private int _n;

        public async Task<Listing> Add(string title, decimal? price = 10, string description = "desc")
        {
            var when = DateTime.UtcNow.AddMinutes(-_n++);   // each later Add is OLDER
            var l = new Listing
            {
                Title = title, Description = description, Price = price,
                CityId = city.Id, CategoryId = category.Id, ContactEmail = "a@b.c",
                CreatedAt = when, UpdatedAt = when,
            };
            Db.Listings.Add(l);
            await Db.SaveChangesAsync();
            return l;
        }

        public Task<PagedResult<Listing>> Browse(SearchFilterVm filter, int pageSize = 20) =>
            Service.BrowseAsync(city.Id, category.Id, filter, pageSize);

        public async Task<List<string>> Titles(SearchFilterVm filter) =>
            (await Browse(filter)).Items.Select(l => l.Title).ToList();
    }

    private async Task<Scope> NewScope()
    {
        var db = fixture.CreateContext();
        var tag = Guid.NewGuid().ToString("N")[..8];
        var city = new City { Name = "C " + tag, Slug = "c-" + tag };
        var category = new Category { Name = "K " + tag, Slug = "k-" + tag, Group = CategoryGroup.ForSale };
        db.AddRange(city, category);
        await db.SaveChangesAsync();
        return new Scope(db, city, category);
    }

    // ---- keyword ----

    [Fact]
    public async Task Keyword_MatchesTitle()
    {
        var s = await NewScope();
        await s.Add("red chair"); await s.Add("blue table");

        Assert.Equal(["red chair"], await s.Titles(new() { Q = "chair" }));
    }

    [Fact]
    public async Task Keyword_MatchesDescription()
    {
        var s = await NewScope();
        await s.Add("thing one", description: "a lovely chair"); await s.Add("thing two", description: "a table");

        Assert.Equal(["thing one"], await s.Titles(new() { Q = "chair" }));
    }

    [Fact]
    public async Task Keyword_IsCaseInsensitive()
    {
        var s = await NewScope();
        await s.Add("Red Chair");

        Assert.Single(await s.Titles(new() { Q = "CHAIR" }));
        Assert.Single(await s.Titles(new() { Q = "chair" }));
    }

    [Fact]
    public async Task Keyword_PercentIsLiteral()
    {
        var s = await NewScope();
        await s.Add("50% off sofa"); await s.Add("500 dollar sofa");

        Assert.Equal(["50% off sofa"], await s.Titles(new() { Q = "50%" }));
    }

    [Fact]
    public async Task Keyword_UnderscoreIsLiteral()
    {
        var s = await NewScope();
        await s.Add("model a_b"); await s.Add("model axb");

        Assert.Equal(["model a_b"], await s.Titles(new() { Q = "a_b" }));
    }

    // ---- price range ----

    [Fact]
    public async Task MinOnly_MaxOnly_Both()
    {
        var s = await NewScope();
        await s.Add("ten", 10); await s.Add("fifty", 50); await s.Add("hundred", 100);

        Assert.Equal(["fifty", "hundred"], (await s.Titles(new() { Min = 50 })).Order());
        Assert.Equal(["fifty", "ten"], (await s.Titles(new() { Max = 50 })).Order());
        Assert.Equal(["fifty"], await s.Titles(new() { Min = 20, Max = 80 }));
    }

    [Fact]
    public async Task UnpricedListing_ExcludedByAnyBound_IncludedWithout()
    {
        var s = await NewScope();
        await s.Add("priced", 10); await s.Add("unpriced", null);

        Assert.Equal(["priced"], await s.Titles(new() { Min = 0 }));
        Assert.Equal(["priced"], await s.Titles(new() { Max = 1000 }));
        Assert.Equal(2, (await s.Titles(new())).Count);
    }

    // ---- sort ----

    [Fact]
    public async Task Sorts_OrderCorrectly_UnpricedLastForPriceSorts()
    {
        var s = await NewScope();
        await s.Add("newest-mid", 50);      // added first = newest
        await s.Add("unpriced", null);
        await s.Add("cheap", 10);
        await s.Add("dear", 100);           // added last = oldest

        Assert.Equal(["newest-mid", "unpriced", "cheap", "dear"], await s.Titles(new() { Sort = ListingSort.Newest }));
        Assert.Equal(["cheap", "newest-mid", "dear", "unpriced"], await s.Titles(new() { Sort = ListingSort.PriceAsc }));
        Assert.Equal(["dear", "newest-mid", "cheap", "unpriced"], await s.Titles(new() { Sort = ListingSort.PriceDesc }));
    }

    // ---- paging ----

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PageBelowOne_ActsAsPageOne(int badPage)
    {
        var s = await NewScope();
        await s.Add("a"); await s.Add("b");

        var page = await s.Browse(new() { Page = badPage });

        Assert.Equal(1, page.Page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task PagePastEnd_IsEmptyButKeepsTotal()
    {
        var s = await NewScope();
        await s.Add("a"); await s.Add("b"); await s.Add("c");

        var page = await s.Browse(new() { Page = 5 }, pageSize: 2);

        Assert.Empty(page.Items);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Theory]
    [InlineData(20, 1)]
    [InlineData(21, 2)]
    public async Task ExactMultiple_Boundary(int count, int expectedPages)
    {
        var s = await NewScope();
        for (var i = 0; i < count; i++) await s.Add($"item {i}");

        var page = await s.Browse(new());

        Assert.Equal(expectedPages, page.TotalPages);
        Assert.Equal(20, page.Items.Count);
    }

    [Fact]
    public async Task PageSize_IsCappedAt100()
    {
        var s = await NewScope();
        await s.Add("a");

        var page = await s.Browse(new(), pageSize: 1000);

        Assert.Equal(100, page.PageSize);
    }

    // ---- everything together ----

    [Fact]
    public async Task KeywordPriceSortAndPage_Compose()
    {
        var s = await NewScope();
        await s.Add("chair 1", 30); await s.Add("chair 2", 20); await s.Add("chair 3", 10);
        await s.Add("chair 4", 200);          // out by price
        await s.Add("table 5", 15);           // out by keyword

        var filter = new SearchFilterVm { Q = "chair", Max = 100, Sort = ListingSort.PriceAsc, Page = 2 };
        var page = await s.Browse(filter, pageSize: 2);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(["chair 1"], page.Items.Select(l => l.Title));   // page 2 of [10, 20, 30]
    }
}
