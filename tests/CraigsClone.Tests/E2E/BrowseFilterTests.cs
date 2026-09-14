using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

// M4.5: the M4 finish line. Keyword + price + sort, then page 2, and the filters survive.
[Trait("Category", "E2E")]
[Collection("app")]
public class BrowseFilterTests(WebAppFixture fixture)
{
    [Fact]
    public async Task FilterThenNext_KeepsEveryFilterInUrlAndForm()
    {
        // boston/volunteers is empty in DevSeeder. 25 rows priced 1..25; min=5 leaves 21 → two pages.
        for (var i = 1; i <= 25; i++)
            await TestData.AddListingAsync(fixture.Services, "boston", "volunteers", title: $"filterme {i:D2}", price: i);

        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/boston/volunteers");
            await page.GetByLabel("search", new() { Exact = true }).FillAsync("filterme");
            await page.GetByLabel("min price").FillAsync("5");
            await page.GetByLabel("sort").SelectOptionAsync(new SelectOptionValue { Label = "price: low to high" });
            await page.GetByRole(AriaRole.Button, new() { Name = "search" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("q=filterme"));
            Assert.Contains("min=5", page.Url);
            Assert.Contains("sort=priceasc", page.Url);
            await Assertions.Expect(page.Locator("li.listing-row")).ToHaveCountAsync(20);
            await Assertions.Expect(page.Locator("li.listing-row").First).ToContainTextAsync("filterme 05");   // cheapest first

            await page.GetByRole(AriaRole.Link, new() { Name = "next", Exact = true }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("page=2"));
            Assert.Contains("q=filterme", page.Url);
            Assert.Contains("min=5", page.Url);
            Assert.Contains("sort=priceasc", page.Url);
            await Assertions.Expect(page.Locator("li.listing-row")).ToHaveCountAsync(1);
            await Assertions.Expect(page.Locator("li.listing-row").First).ToContainTextAsync("filterme 25");
            await Assertions.Expect(page.GetByLabel("search", new() { Exact = true })).ToHaveValueAsync("filterme");
            await Assertions.Expect(page.GetByLabel("sort")).ToHaveValueAsync("priceasc");
        }
        finally { await page.CloseAsync(); }
    }
}
