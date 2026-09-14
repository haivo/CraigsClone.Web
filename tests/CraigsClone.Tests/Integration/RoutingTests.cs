using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration;

// M0.7: attribute routing only, template extras removed.
[Trait("Category", "Integration")]
[Collection("app")]
public class RoutingTests(WebAppFixture fixture)
{
    [Theory]
    [InlineData("/", HttpStatusCode.OK)]
    [InlineData("/Home/Index", HttpStatusCode.NotFound)]     // conventional route is gone
    [InlineData("/Home/Privacy", HttpStatusCode.NotFound)]   // page deleted
    [InlineData("/lib/bootstrap/dist/css/bootstrap.min.css", HttpStatusCode.NotFound)] // folder deleted
    [InlineData("/css/site.css", HttpStatusCode.OK)]
    public async Task Url_ReturnsExpectedStatus(string url, HttpStatusCode expected)
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(expected, response.StatusCode);
    }
}
