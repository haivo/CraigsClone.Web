using CraigsClone.Tests.E2E;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CraigsClone.Tests.Integration;

// M0.6: the connection string is read from configuration and can be overridden
// (the fixture replaces it with the Testcontainers one).
[Trait("Category", "Integration")]
[Collection("app")]
public class ConfigurationTests(WebAppFixture fixture)
{
    [Fact]
    public void ConnectionString_IsPresentAndOverridden()
    {
        var config = fixture.Services.GetRequiredService<IConfiguration>();

        var cs = config.GetConnectionString("Default");

        Assert.False(string.IsNullOrWhiteSpace(cs));
        Assert.Contains("Host=", cs);
        Assert.DoesNotContain("Port=5433", cs);   // dev value replaced by the test container's port
    }
}
