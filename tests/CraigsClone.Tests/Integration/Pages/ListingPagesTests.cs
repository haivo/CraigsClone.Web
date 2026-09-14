using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class ListingPagesTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Browse_KnownCityAndCategory_Is200()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/austin/furniture");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("furniture", body);
        Assert.Contains("Austin", body);
    }

    [Theory]
    [InlineData("/austin/nope")]
    [InlineData("/nope/furniture")]
    [InlineData("/listing/999999")]
    [InlineData("/listing/abc")]      // fails the :int constraint, then fails as a city slug
    public async Task UnknownThings_Are404(string url)
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Browse_ShowsInsertedListing()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "boston", "bikes");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/boston/bikes");

        Assert.Contains(listing.Title, body);
        Assert.Contains($"href=\"/listing/{listing.Id}\"", body);
    }

    [Fact]
    public async Task Details_ShowsAllFields()
    {
        var listing = await TestData.AddListingAsync(fixture.Services,
            price: 1500, description: "Solid oak, some scratches.", neighborhood: "Hyde Park");
        using var client = fixture.CreateClient();

        var response = await client.GetAsync($"/listing/{listing.Id}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(listing.Title, body);
        Assert.Contains("$1,500", body);
        Assert.Contains("Solid oak, some scratches.", body);
        Assert.Contains("seller@example.com", body);
        Assert.Contains("Hyde Park", body);
        Assert.Contains("href=\"/austin/furniture\"", body);   // back to the category
    }
}
