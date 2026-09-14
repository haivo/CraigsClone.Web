using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class ErrorPagesTests(WebAppFixture fixture)
{
    [Theory]
    [InlineData("/nowhere/nothing")]
    [InlineData("/listing/999999")]
    [InlineData("/nowhere")]
    public async Task NotFound_KeepsStatus_AndRendersFriendlyPageInLayout(string url)
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("page not found", body);
        Assert.Contains("<header>", body);            // inside the layout, not a bare response
        Assert.Contains("href=\"/\"", body);          // way home
    }

    [Fact]
    public async Task ErrorRouteDirectly_Works()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/error/404");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("page not found", body);
    }

    [Fact]
    public async Task OtherStatus_GetsGenericPage()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/error/500");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("something went wrong", body);
    }

    // M0.7's template leftover is gone.
    [Fact]
    public async Task OldHomeError_IsGone()
    {
        using var client = fixture.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Home/Error")).StatusCode);
    }
}
