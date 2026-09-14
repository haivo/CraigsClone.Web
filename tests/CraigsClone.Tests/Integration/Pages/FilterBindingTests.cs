using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

// M4.1: model binding is framework behaviour, so prove it with real requests.
// Meaningful only once M4.5 binds SearchFilterVm in Listings.Index and renders the sort in the form.
[Trait("Category", "Integration")]
[Collection("app")]
public class FilterBindingTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Sort_BindsCaseInsensitively()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/austin/furniture?sort=PRICEASC");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<option value=\"priceasc\" selected", body);
    }

    // Decision: an invalid sort falls back to newest rather than erroring.
    [Fact]
    public async Task InvalidSort_FallsBackToNewest()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/austin/furniture?sort=banana");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<option value=\"newest\" selected", body);
    }

    [Fact]
    public async Task InvalidPage_IsTreatedAsPageOne()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/austin/furniture?page=abc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
