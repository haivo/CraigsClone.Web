using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CraigsClone.Tests.E2E;

/// <summary>Inserts rows into the running app's database, for HTTP and browser tests.</summary>
public static class TestData
{
    public static string UniqueTitle(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    public static async Task<Listing> AddListingAsync(
        IServiceProvider services,
        string citySlug = "austin",
        string categorySlug = "furniture",
        string? title = null,
        decimal? price = 25,
        string description = "A test listing.",
        string? neighborhood = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var city = await db.Cities.SingleAsync(c => c.Slug == citySlug);
        var category = await db.Categories.SingleAsync(c => c.Slug == categorySlug);
        var now = DateTime.UtcNow;

        var listing = new Listing
        {
            Title = title ?? UniqueTitle("Test"),
            Description = description,
            Price = price,
            CityId = city.Id,
            CategoryId = category.Id,
            Neighborhood = neighborhood,
            ContactEmail = "seller@example.com",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Listings.Add(listing);
        await db.SaveChangesAsync();
        return listing;
    }

    /// <summary>
    /// Inserts many listings in ONE round trip. Titles are "{prefix} 01" .. "{prefix} NN"; earlier numbers are newer.
    /// Use this instead of looping AddListingAsync: 25 separate scopes cost seconds, this costs milliseconds.
    /// </summary>
    public static async Task AddListingsAsync(
        IServiceProvider services,
        string citySlug,
        string categorySlug,
        int count,
        string titlePrefix,
        Func<int, decimal?>? price = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var city = await db.Cities.SingleAsync(c => c.Slug == citySlug);
        var category = await db.Categories.SingleAsync(c => c.Slug == categorySlug);
        var now = DateTime.UtcNow;

        for (var i = 1; i <= count; i++)
        {
            var when = now.AddSeconds(-i);
            db.Listings.Add(new Listing
            {
                Title = $"{titlePrefix} {i:D2}",
                Description = "A test listing.",
                Price = price is null ? 25 : price(i),
                CityId = city.Id,
                CategoryId = category.Id,
                ContactEmail = "seller@example.com",
                CreatedAt = when,
                UpdatedAt = when,
            });
        }
        await db.SaveChangesAsync();
    }
}
