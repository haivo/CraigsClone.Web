using CraigsClone.Web.Data;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Services;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class ListingServiceWriteTests(PostgresFixture fixture)
{
    // Cities and categories are seeded by the fixture, so real ids are available.
    private static async Task<ListingFormVm> FormAsync(AppDbContext db, string title) => new()
    {
        Title = title,
        Description = "desc",
        Price = 10,
        CityId = await db.Cities.Where(c => c.Slug == "austin").Select(c => c.Id).SingleAsync(),
        CategoryId = await db.Categories.Where(c => c.Slug == "furniture").Select(c => c.Id).SingleAsync(),
        ContactEmail = "a@b.c",
    };

    private static string Unique() => "write " + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task Create_SavesRow_WithUtcTimestamps()
    {
        await using var db = fixture.CreateContext();
        var before = DateTime.UtcNow;

        var created = await new ListingService(db).CreateAsync(await FormAsync(db, Unique()));

        Assert.True(created.Id > 0);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, created.CreatedAt.Kind);
        Assert.InRange(created.CreatedAt, before.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

        await using var fresh = fixture.CreateContext();
        Assert.NotNull(await fresh.Listings.FindAsync(created.Id));
    }

    [Fact]
    public async Task Update_ChangesFields_AndBumpsUpdatedAt()
    {
        await using var db = fixture.CreateContext();
        var service = new ListingService(db);
        var created = await service.CreateAsync(await FormAsync(db, Unique()));
        await Task.Delay(20);   // so the clocks differ

        var form = await FormAsync(db, Unique() + " edited");
        form.Price = 99;
        var ok = await service.UpdateAsync(created.Id, form);

        Assert.True(ok);
        await using var fresh = fixture.CreateContext();
        var stored = await fresh.Listings.SingleAsync(l => l.Id == created.Id);
        Assert.Equal(form.Title, stored.Title);
        Assert.Equal(99m, stored.Price);
        Assert.True(stored.UpdatedAt > stored.CreatedAt);
        // Never touched by update. Postgres stores microseconds, .NET keeps 100ns ticks,
        // so compare with a tolerance rather than exact equality.
        Assert.Equal(created.CreatedAt, stored.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsFalse()
    {
        await using var db = fixture.CreateContext();

        var ok = await new ListingService(db).UpdateAsync(999_999, await FormAsync(db, Unique()));

        Assert.False(ok);
    }

    [Fact]
    public async Task Delete_RemovesRow()
    {
        await using var db = fixture.CreateContext();
        var service = new ListingService(db);
        var created = await service.CreateAsync(await FormAsync(db, Unique()));

        var ok = await service.DeleteAsync(created.Id);

        Assert.True(ok);
        Assert.Null(await service.GetAsync(created.Id));
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsFalse()
    {
        await using var db = fixture.CreateContext();

        Assert.False(await new ListingService(db).DeleteAsync(999_999));
    }
}
