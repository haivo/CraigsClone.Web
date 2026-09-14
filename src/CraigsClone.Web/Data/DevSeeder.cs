using CraigsClone.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Data;

/// <summary>
/// Sample ads so pages aren't empty during development. Runs only in Development (see Program.cs)
/// and only once: if the first sample's title is already in the table, it does nothing.
/// </summary>
public static class DevSeeder
{
    public record Sample(string City, string Category, string Title, decimal? Price, int DaysAgo,
        string Description, string? Neighborhood = null);

    public static readonly IReadOnlyList<Sample> Samples =
    [
        // Austin
        new("austin", "furniture", "Mid-century walnut dresser", 220, 0, "Six drawers, solid wood, minor wear on top. Pickup only.", "Hyde Park"),
        new("austin", "furniture", "IKEA Kallax 4x4 shelf, white", 60, 1, "Assembled, good condition. You haul.", "East Austin"),
        new("austin", "furniture", "Green velvet sofa", 450, 3, "Seats three, very comfortable, no pets or smoke.", "South Congress"),
        new("austin", "furniture", "Oak dining table + 4 chairs", 300, 6, "Some scratches on the table top. Chairs are solid."),
        new("austin", "bikes", "Trek FX 3 hybrid, 56cm", 380, 1, "Ridden two summers, new tires last month.", "Mueller"),
        new("austin", "bikes", "Kids 20\" bike, blue", 45, 4, "Outgrown. Training wheels included."),
        new("austin", "electronics", "27\" Dell monitor, 1440p", 140, 2, "USB-C, works great, no dead pixels."),
        new("austin", "electronics", "Nintendo Switch + 3 games", 180, 5, "Original model, comes with dock and two Joy-Cons."),
        new("austin", "apartments", "1BR near UT, available Oct 1", 1350, 0, "Second floor, laundry in unit, parking included.", "West Campus"),
        new("austin", "apartments", "2BR/2BA with balcony", 1900, 2, "Pool, gym, dog friendly. 12 month lease."),
        new("austin", "software", "Junior .NET developer, hybrid", null, 1, "Small team, C# and Postgres, two days a week in office."),
        new("austin", "events", "Saturday farmers market volunteers", null, 3, "Help set up tables 7-9am. Free coffee."),

        // Boston
        new("boston", "furniture", "Queen bed frame, metal", 90, 0, "No box spring needed. Disassembles for transport.", "Somerville"),
        new("boston", "furniture", "Standing desk, 48\"", 160, 4, "Electric, two memory presets."),
        new("boston", "bikes", "Vintage Raleigh 3-speed", 150, 2, "Rides well, original leather saddle.", "Cambridge"),
        new("boston", "bikes", "Road bike, needs tune-up", 75, 7, "Shifters sticky, otherwise sound."),
        new("boston", "rooms", "Room in 3BR, Allston", 950, 1, "Shared kitchen and bath, utilities split three ways.", "Allston"),
        new("boston", "lost-found", "Found: grey cat near Davis Sq", null, 0, "Friendly, no collar. Call to describe.", "Davis Square"),

        // Chicago
        new("chicago", "cars-trucks", "2012 Honda Civic, 140k miles", 5800, 1, "One owner, clean title, recent brakes.", "Logan Square"),
        new("chicago", "cars-trucks", "2008 Ford F-150, work truck", 4200, 5, "Runs strong, some rust on the bed."),
        new("chicago", "free", "Free moving boxes", null, 0, "About 30, mostly medium. On the porch, take them.", "Pilsen"),
        new("chicago", "free", "Free piano, you move it", null, 3, "Upright, needs tuning. Second floor, no elevator."),
        new("chicago", "retail", "Weekend cashier, bookstore", null, 2, "Saturdays and Sundays, 10-6. Must like books."),

        // Denver
        new("denver", "bikes", "Specialized Rockhopper 29er", 420, 0, "Hardtail, tubeless, ready for trails.", "Capitol Hill"),
        new("denver", "bikes", "Cargo bike, electric assist", 1500, 6, "Carries two kids, battery holds about 25 miles."),
        new("denver", "computer-gigs", "Fix my WordPress site", 200, 1, "Contact form broken after an update. Quick job for the right person."),
        new("denver", "lessons", "Guitar lessons, beginner", 40, 2, "Per hour, your place or mine. Acoustic or electric."),

        // Portland
        new("portland", "furniture", "Bookshelf, pine, 6ft", 50, 1, "Five shelves, a little wobbly.", "Alberta"),
        new("portland", "electronics", "Canon EOS Rebel T7 kit", 350, 3, "Body, 18-55 lens, bag, two batteries."),
        new("portland", "sublets", "Studio sublet, Nov-Feb", 1100, 0, "Furnished, near the streetcar. Cat okay.", "Pearl District"),

        // Seattle
        new("seattle", "software", "Backend engineer, Go or C#", null, 0, "Remote friendly, health insurance, small startup.", "Fremont"),
        new("seattle", "software", "QA contractor, 3 months", null, 4, "Manual and Playwright testing for a web app."),
        new("seattle", "apartments", "Basement 1BR, private entrance", 1450, 2, "Quiet street, includes water and garbage.", "Ballard"),
        new("seattle", "volunteers", "Trail maintenance, Sunday", null, 1, "Bring gloves. Tools and lunch provided."),
        new("seattle", "creative-gigs", "Logo for a coffee cart", 250, 3, "Simple, one or two colors. Need it in two weeks."),
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        var sentinel = Samples[0].Title;
        if (await db.Listings.AnyAsync(l => l.Title == sentinel)) return;

        var cities = await db.Cities.ToDictionaryAsync(c => c.Slug, c => c.Id);
        var categories = await db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id);
        var now = DateTime.UtcNow;

        for (var i = 0; i < Samples.Count; i++)
        {
            var s = Samples[i];
            var when = now.AddDays(-s.DaysAgo).AddMinutes(-i);   // stable, distinct ordering
            db.Listings.Add(new Listing
            {
                Title = s.Title,
                Description = s.Description,
                Price = s.Price,
                CityId = cities[s.City],
                CategoryId = categories[s.Category],
                Neighborhood = s.Neighborhood,
                ContactEmail = "seller@example.com",
                CreatedAt = when,
                UpdatedAt = when,
            });
        }

        await db.SaveChangesAsync();
    }
}
