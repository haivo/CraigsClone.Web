using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

// M3.3: a POST without the anti-forgery token is rejected before the action runs.
// It must be 400, not 404: the filter runs first, so even a missing listing id gets 400.
[Trait("Category", "Integration")]
[Collection("app")]
public class AntiforgeryTests(WebAppFixture fixture)
{
    private static FormUrlEncodedContent SomeForm() => new(new Dictionary<string, string>
    {
        ["Title"] = "x", ["Description"] = "y", ["CityId"] = "1", ["CategoryId"] = "1", ["ContactEmail"] = "a@b.c",
    });

    [Fact]
    public async Task PostCreate_WithoutToken_Is400()
    {
        using var client = fixture.CreateClient();

        var response = await client.PostAsync("/post", SomeForm());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Route arrives in M3.5")]
    public async Task PostEdit_WithoutToken_Is400()
    {
        using var client = fixture.CreateClient();

        var response = await client.PostAsync("/listing/1/edit", SomeForm());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Route arrives in M3.6")]
    public async Task PostDelete_WithoutToken_Is400()
    {
        using var client = fixture.CreateClient();

        var response = await client.PostAsync("/listing/1/delete", new StringContent(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
