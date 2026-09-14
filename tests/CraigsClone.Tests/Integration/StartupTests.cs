using CraigsClone.Tests.E2E;
using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CraigsClone.Tests.Integration;

// What the running app does at startup: DbContext registration (M1.3), migrate + seed (M1.6).
[Trait("Category", "Integration")]
[Collection("app")]
public class StartupTests(WebAppFixture fixture)
{
    // Proves three things at once: the context is registered, the connection string
    // setting is read, and the app really talks to the Postgres the fixture started.
    [Fact]
    public async Task App_CanReachDatabase()
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await db.Database.CanConnectAsync());
    }
}
