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
}
