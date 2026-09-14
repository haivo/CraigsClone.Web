using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Data;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class DbSeederTests(PostgresFixture fixture)
{
    // Running the seeder twice must not duplicate anything.
    // Other tests add their own cities with random slugs to this database,
    // so count only the seeded slugs rather than the whole table.
    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        await using (var db = fixture.CreateContext())
        {
            await DbSeeder.SeedAsync(db);
        }
        await using (var db = fixture.CreateContext())
        {
            await DbSeeder.SeedAsync(db);
        }

        await using var check = fixture.CreateContext();
        var citySlugs = DbSeeder.Cities.Select(c => c.Slug).ToList();
        var categorySlugs = DbSeeder.Categories.Select(c => c.Slug).ToList();

        Assert.Equal(6, await check.Cities.CountAsync(c => citySlugs.Contains(c.Slug)));
        Assert.Equal(22, await check.Categories.CountAsync(c => categorySlugs.Contains(c.Slug)));
    }

    [Fact]
    public async Task SeedAsync_StoresGroupAsText()
    {
        await using var db = fixture.CreateContext();
        await DbSeeder.SeedAsync(db);

        var raw = await db.Database.SqlQueryRaw<string>(
            "select \"Group\" as \"Value\" from \"Categories\" where \"Slug\" = 'furniture'").SingleAsync();

        Assert.Equal("ForSale", raw);
    }
}
