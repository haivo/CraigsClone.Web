using Npgsql;

namespace CraigsClone.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("postgres")]
public class PostgresFixtureTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Container_AnswersSelectOne()
    {
        await using var conn = new NpgsqlConnection(fixture.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("select 1", conn);

        var result = await cmd.ExecuteScalarAsync();

        Assert.Equal(1, result);
    }
}
