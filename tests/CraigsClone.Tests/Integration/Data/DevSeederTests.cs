using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Data;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class DevSeederTests(PostgresFixture fixture)
{
    // Other tests add their own listings to this database, so count only the sample titles.
    [Fact]
    public async Task SeedAsync_RunTwice_InsertsSamplesOnce()
    {
        await using (var db = fixture.CreateContext()) await DevSeeder.SeedAsync(db);
        await using (var db = fixture.CreateContext()) await DevSeeder.SeedAsync(db);

        await using var check = fixture.CreateContext();
        var titles = DevSeeder.Samples.Select(s => s.Title).ToList();
        var count = await check.Listings.CountAsync(l => titles.Contains(l.Title));

        Assert.Equal(DevSeeder.Samples.Count, count);
    }

    [Fact]
    public async Task SeededListings_HaveUtcTimestamps()
    {
        await using var db = fixture.CreateContext();
        await DevSeeder.SeedAsync(db);

        var sentinel = DevSeeder.Samples[0].Title;
        var listing = await db.Listings.AsNoTracking().FirstAsync(l => l.Title == sentinel);

        Assert.Equal(DateTimeKind.Utc, listing.CreatedAt.Kind);
        Assert.Equal(listing.CreatedAt, listing.UpdatedAt);
    }
}
