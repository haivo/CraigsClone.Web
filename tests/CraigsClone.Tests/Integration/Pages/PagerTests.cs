using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

// M4.3: the pager renders on the category page. Needs M4.5 to render it, and uses
// city/category combos that DevSeeder leaves empty so the counts are exact.
[Trait("Category", "Integration")]
[Collection("app")]
public class PagerTests(WebAppFixture fixture)
{
    private static Task AddMany(IServiceProvider services, string city, string category, int count, string prefix) =>
        TestData.AddListingsAsync(services, city, category, count, prefix);

    [Fact]
    public async Task TwentyFiveListings_ShowPageTwoAndNext()
    {
        await AddMany(fixture.Services, "denver", "lost-found", 25, "pager");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/denver/lost-found");

        Assert.Contains("href=\"?page=2\"", body);
        Assert.Contains(">next</a>", body);
        Assert.DoesNotContain(">prev</a>", body);
    }

    [Fact]
    public async Task FiveListings_ShowNoPager()
    {
        await AddMany(fixture.Services, "denver", "volunteers", 5, "nopager");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/denver/volunteers");

        Assert.DoesNotContain("class=\"pager\"", body);
        Assert.DoesNotContain("page=2", body);
    }

    [Fact]
    public async Task PageTwoWithKeyword_PrevLinkKeepsKeyword()
    {
        await AddMany(fixture.Services, "portland", "lost-found", 25, "keepq");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/portland/lost-found?q=keepq&page=2");

        Assert.Contains("href=\"?q=keepq\"", body);            // prev: page 1, so no page param
        Assert.Contains("aria-current=\"page\">2</span>", body);
    }
}
