using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Data;

/// <summary>
/// Inserts the fixed cities and categories. Safe to run on every startup:
/// anything whose slug already exists is left alone.
/// </summary>
public static class DbSeeder
{
    // These words are routes, so no city or category slug may use them.
    public static readonly IReadOnlySet<string> ReservedSlugs =
        new HashSet<string> { "post", "search", "listing", "error" };

    public static readonly IReadOnlyList<(string Name, string Slug)> Cities =
    [
        ("Austin", "austin"),
        ("Boston", "boston"),
        ("Chicago", "chicago"),
        ("Denver", "denver"),
        ("Portland", "portland"),
        ("Seattle", "seattle"),
    ];

    public static readonly IReadOnlyList<(CategoryGroup Group, string Name, string Slug)> Categories =
    [
        (CategoryGroup.ForSale, "furniture", "furniture"),
        (CategoryGroup.ForSale, "electronics", "electronics"),
        (CategoryGroup.ForSale, "bikes", "bikes"),
        (CategoryGroup.ForSale, "cars & trucks", "cars-trucks"),
        (CategoryGroup.ForSale, "free stuff", "free"),

        (CategoryGroup.Housing, "apts / housing for rent", "apartments"),
        (CategoryGroup.Housing, "rooms & shares", "rooms"),
        (CategoryGroup.Housing, "sublets & temporary", "sublets"),
        (CategoryGroup.Housing, "housing wanted", "housing-wanted"),

        (CategoryGroup.Jobs, "software / QA / DBA", "software"),
        (CategoryGroup.Jobs, "food / bev / hospitality", "food-hospitality"),
        (CategoryGroup.Jobs, "retail / wholesale", "retail"),
        (CategoryGroup.Jobs, "general labor", "general-labor"),

        (CategoryGroup.Services, "computer", "computer-services"),
        (CategoryGroup.Services, "household", "household-services"),
        (CategoryGroup.Services, "lessons & tutoring", "lessons"),

        (CategoryGroup.Community, "events", "events"),
        (CategoryGroup.Community, "lost & found", "lost-found"),
        (CategoryGroup.Community, "volunteers", "volunteers"),

        (CategoryGroup.Gigs, "computer gigs", "computer-gigs"),
        (CategoryGroup.Gigs, "labor gigs", "labor-gigs"),
        (CategoryGroup.Gigs, "creative gigs", "creative-gigs"),
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        var existingCities = await db.Cities.Select(c => c.Slug).ToHashSetAsync();
        foreach (var (name, slug) in Cities)
        {
            if (!existingCities.Contains(slug))
                db.Cities.Add(new City { Name = name, Slug = slug });
        }

        var existingCategories = await db.Categories.Select(c => c.Slug).ToHashSetAsync();
        foreach (var (group, name, slug) in Categories)
        {
            if (!existingCategories.Contains(slug))
                db.Categories.Add(new Category { Group = group, Name = name, Slug = slug });
        }

        await db.SaveChangesAsync();
    }
}
