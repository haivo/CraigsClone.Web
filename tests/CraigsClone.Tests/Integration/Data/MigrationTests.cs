using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Integration.Data;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class MigrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task InitialMigration_IsApplied()
    {
        await using var db = fixture.CreateContext();

        var applied = await db.Database.GetAppliedMigrationsAsync();

        Assert.Contains(applied, m => m.EndsWith("_Initial"));
    }

    [Fact]
    public async Task NoMigrations_ArePending()
    {
        await using var db = fixture.CreateContext();

        var pending = await db.Database.GetPendingMigrationsAsync();

        Assert.Empty(pending);
    }
}
