using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Services;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class ListingServiceReadTests(PostgresFixture fixture)
{
    // Each test gets its own city + category so its rows can't collide with anyone else's.
    private static async Task<(City City, Category Category)> OwnCityAndCategory(AppDbContext db)
    {
        var tag = Guid.NewGuid().ToString("N")[..8];
        var city = new City { Name = "City " + tag, Slug = "city-" + tag };
        var category = new Category { Name = "Cat " + tag, Slug = "cat-" + tag, Group = CategoryGroup.ForSale };
        db.AddRange(city, category);
        await db.SaveChangesAsync();
        return (city, category);
    }

    private static async Task<List<Listing>> AddListings(AppDbContext db, City city, Category category, int count)
    {
        var now = DateTime.UtcNow;
        var listings = Enumerable.Range(1, count).Select(i => new Listing
        {
            Title = $"Item {i:D2}",
            Description = "desc",
            CityId = city.Id,
            CategoryId = category.Id,
            ContactEmail = "a@b.c",
            CreatedAt = now.AddMinutes(-i),   // Item 01 is the newest
            UpdatedAt = now.AddMinutes(-i),
        }).ToList();
        db.AddRange(listings);
        await db.SaveChangesAsync();
        return listings;
    }

    [Fact]
    public async Task Browse_FirstPage_Has20NewestFirst()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        await AddListings(db, city, category, 25);
        var service = new ListingService(db);

        var page = await service.BrowseAsync(city.Id, category.Id, page: 1);

        Assert.Equal(20, page.Items.Count);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal("Item 01", page.Items[0].Title);
        Assert.Equal("Item 20", page.Items[^1].Title);
    }

    [Fact]
    public async Task Browse_SecondPage_HasRemainingFive()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        await AddListings(db, city, category, 25);
        var service = new ListingService(db);

        var page = await service.BrowseAsync(city.Id, category.Id, page: 2);

        Assert.Equal(5, page.Items.Count);
        Assert.Equal("Item 21", page.Items[0].Title);
        Assert.False(page.HasNext);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Browse_PageBelowOne_ActsAsPageOne(int badPage)
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        await AddListings(db, city, category, 3);
        var service = new ListingService(db);

        var page = await service.BrowseAsync(city.Id, category.Id, badPage);

        Assert.Equal(1, page.Page);
        Assert.Equal(3, page.Items.Count);
    }

    [Fact]
    public async Task Browse_PagePastEnd_IsEmptyButKeepsTotal()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        await AddListings(db, city, category, 25);
        var service = new ListingService(db);

        var page = await service.BrowseAsync(city.Id, category.Id, page: 99);

        Assert.Empty(page.Items);
        Assert.Equal(25, page.TotalCount);
    }

    [Fact]
    public async Task Browse_ExcludesOtherCategories()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        var (_, otherCategory) = await OwnCityAndCategory(db);
        await AddListings(db, city, category, 2);
        await AddListings(db, city, otherCategory, 3);
        var service = new ListingService(db);

        var page = await service.BrowseAsync(city.Id, category.Id, page: 1);

        Assert.Equal(2, page.TotalCount);
        Assert.All(page.Items, l => Assert.Equal(category.Id, l.CategoryId));
    }

    [Fact]
    public async Task Get_LoadsCityAndCategory()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await OwnCityAndCategory(db);
        var listing = (await AddListings(db, city, category, 1)).Single();

        await using var fresh = fixture.CreateContext();   // no tracked entities to cheat with
        var found = await new ListingService(fresh).GetAsync(listing.Id);

        Assert.NotNull(found);
        Assert.Equal(city.Slug, found.City.Slug);
        Assert.Equal(category.Slug, found.Category.Slug);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsNull()
    {
        await using var db = fixture.CreateContext();

        var found = await new ListingService(db).GetAsync(999_999);

        Assert.Null(found);
    }
}
