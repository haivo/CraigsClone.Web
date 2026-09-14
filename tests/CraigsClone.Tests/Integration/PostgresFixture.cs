using Testcontainers.PostgreSql;

namespace CraigsClone.Tests.Integration;

/// <summary>
/// Starts one throwaway Postgres 16 container for the whole test run.
/// Grows in M1.2 (CreateContext) and M1.5 (migrate + seed).
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16").Build();

    public string ConnectionString => _pg.GetConnectionString();

    public Task InitializeAsync() => _pg.StartAsync();

    public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
}

[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
