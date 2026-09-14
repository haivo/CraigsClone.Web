using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class LayoutTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Home_HasHeaderWithPostLink_AndNoBootstrap()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/");

        Assert.Contains("<header>", body);
        Assert.Contains("href=\"/post\"", body);
        Assert.DoesNotContain("bootstrap", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Home_HasNoCurrentCityLink()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/");

        Assert.DoesNotContain("class=\"city\"", body);
    }

    [Theory]
    [InlineData("/austin")]
    [InlineData("/austin/furniture")]
    public async Task CityPages_ShowCurrentCityInHeader(string url)
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync(url);

        Assert.Contains("class=\"city\" href=\"/austin\">Austin</a>", body);
    }

    [Fact]
    public async Task Details_ShowsListingsCityInHeader()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "denver", "bikes");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/listing/{listing.Id}");

        Assert.Contains("class=\"city\" href=\"/denver\">Denver</a>", body);
    }

    [Fact]
    public async Task Stylesheet_IsServedAsCss()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/css/site.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("text/css", response.Content.Headers.ContentType?.MediaType);
    }
}
