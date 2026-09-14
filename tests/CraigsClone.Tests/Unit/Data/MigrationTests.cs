using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Tests.Unit.Data;

[Trait("Category", "Unit")]
public class MigrationTests
{
    // Fails whenever an entity or OnModelCreating changes without a new migration.
    // Compares the model to the migration snapshot; no database needed.
    [Fact]
    public void Model_HasNoPendingChanges()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused").Options);

        Assert.False(db.Database.HasPendingModelChanges(),
            "The EF model differs from the last migration. Run: dotnet ef migrations add <Name> -o Data/Migrations");
    }
}
