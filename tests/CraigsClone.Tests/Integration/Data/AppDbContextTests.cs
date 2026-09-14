using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Data;

// These prove the mapping reached the real database, not just the EF model.
[Trait("Category", "Integration")]
[Collection("postgres")]
public class AppDbContextTests(PostgresFixture fixture)
{
    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    [Fact]
    public async Task DuplicateCitySlug_IsRejectedByDatabase()
    {
        var slug = Unique("dup");
        await using var db = fixture.CreateContext();
        db.Cities.Add(new City { Name = "One", Slug = slug });
        await db.SaveChangesAsync();

        db.Cities.Add(new City { Name = "Two", Slug = slug });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Price_IsStoredWithTwoDecimals()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await AddCityAndCategory(db);
        var listing = NewListing(city, category);
        listing.Price = 12.345m;
        db.Listings.Add(listing);
        await db.SaveChangesAsync();

        await using var fresh = fixture.CreateContext();
        var stored = await fresh.Listings.Where(l => l.Id == listing.Id).Select(l => l.Price).SingleAsync();

        Assert.NotNull(stored);
        Assert.Contains(stored.Value, new[] { 12.34m, 12.35m });
    }

    // Point 4 of 00-before-you-start.md: Npgsql refuses non-UTC DateTime values.
    [Fact]
    public async Task LocalDateTime_IsRejected()
    {
        await using var db = fixture.CreateContext();
        var (city, category) = await AddCityAndCategory(db);
        var listing = NewListing(city, category);
        listing.CreatedAt = listing.UpdatedAt = DateTime.Now;   // Kind = Local
        db.Listings.Add(listing);

        await Assert.ThrowsAnyAsync<Exception>(() => db.SaveChangesAsync());
    }

    private static async Task<(City, Category)> AddCityAndCategory(Web.Data.AppDbContext db)
    {
        var city = new City { Name = "Test City", Slug = Unique("city") };
        var category = new Category { Name = "Test Cat", Slug = Unique("cat"), Group = CategoryGroup.ForSale };
        db.AddRange(city, category);
        await db.SaveChangesAsync();
        return (city, category);
    }

    private static Listing NewListing(City city, Category category) => new()
    {
        Title = "t",
        Description = "d",
        CityId = city.Id,
        CategoryId = category.Id,
        ContactEmail = "a@b.c",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };
}
