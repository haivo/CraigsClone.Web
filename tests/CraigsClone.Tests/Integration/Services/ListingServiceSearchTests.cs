using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Services;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class ListingServiceSearchTests(PostgresFixture fixture)
{
    // One row in each of Austin/furniture, Austin/bikes, Boston/bikes, all sharing a unique keyword.
    private static async Task<(string Keyword, int Austin, int Boston, int Furniture, int Bikes)> Seed(AppDbContext db)
    {
        var keyword = "srch" + Guid.NewGuid().ToString("N")[..8];
        var austin = await db.Cities.SingleAsync(c => c.Slug == "austin");
        var boston = await db.Cities.SingleAsync(c => c.Slug == "boston");
        var furniture = await db.Categories.SingleAsync(c => c.Slug == "furniture");
        var bikes = await db.Categories.SingleAsync(c => c.Slug == "bikes");

        Listing Row(City c, Category k, string title) => new()
        {
            Title = $"{title} {keyword}", Description = "d", CityId = c.Id, CategoryId = k.Id,
            ContactEmail = "a@b.c", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        db.AddRange(Row(austin, furniture, "austin-furniture"), Row(austin, bikes, "austin-bikes"), Row(boston, bikes, "boston-bikes"));
        await db.SaveChangesAsync();
        return (keyword, austin.Id, boston.Id, furniture.Id, bikes.Id);
    }

    private static List<string> Names(PagedResult<Listing> page) =>
        page.Items.Select(l => l.Title.Split(' ')[0]).Order().ToList();

    [Fact]
    public async Task NoScope_FindsAll()
    {
        await using var db = fixture.CreateContext();
        var s = await Seed(db);

        var page = await new ListingService(db).SearchAsync(new() { Q = s.Keyword }, cityId: null, categoryId: null);

        Assert.Equal(["austin-bikes", "austin-furniture", "boston-bikes"], Names(page));
    }

    [Fact]
    public async Task CityScope_FindsThatCityOnly()
    {
        await using var db = fixture.CreateContext();
        var s = await Seed(db);

        var page = await new ListingService(db).SearchAsync(new() { Q = s.Keyword }, s.Austin, categoryId: null);

        Assert.Equal(["austin-bikes", "austin-furniture"], Names(page));
    }

    [Fact]
    public async Task CityAndCategoryScope_FindsOne()
    {
        await using var db = fixture.CreateContext();
        var s = await Seed(db);

        var page = await new ListingService(db).SearchAsync(new() { Q = s.Keyword }, s.Austin, s.Bikes);

        Assert.Equal(["austin-bikes"], Names(page));
    }

    [Fact]
    public async Task Results_HaveCityAndCategoryLoaded()
    {
        await using var db = fixture.CreateContext();
        var s = await Seed(db);

        await using var fresh = fixture.CreateContext();
        var page = await new ListingService(fresh).SearchAsync(new() { Q = s.Keyword }, null, null);

        Assert.All(page.Items, l =>
        {
            Assert.NotNull(l.City);
            Assert.NotNull(l.Category);
        });
    }
}
