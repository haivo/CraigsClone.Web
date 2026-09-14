using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class HomePagesTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Home_ListsCities()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"/austin\"", body);
        Assert.Contains("href=\"/seattle\"", body);
    }

    [Fact]
    public async Task Home_ShowsGroupsWithoutLinks()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/");

        Assert.Contains("for sale", body);
        Assert.Contains("furniture", body);
        Assert.DoesNotContain("href=\"/austin/furniture\"", body);   // no city chosen yet
    }

    [Fact]
    public async Task City_ShowsCategoryLinks()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/austin");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Austin", body);
        Assert.Contains("href=\"/austin/furniture\"", body);
        Assert.Contains("href=\"/austin/creative-gigs\"", body);
    }

    [Theory]
    [InlineData("/nowhere")]
    [InlineData("/Austin")]   // slugs are case-sensitive; pinned so it's a decision, not an accident
    public async Task UnknownCity_Is404(string url)
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
