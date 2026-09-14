using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CraigsClone.Tests.Integration;

/// <summary>
/// Starts one throwaway Postgres 16 container for the whole test run, applies the
/// migrations (the same way production would), and seeds cities and categories.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16").Build();

    public string ConnectionString => _pg.GetConnectionString();

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }

    public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
}

[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
