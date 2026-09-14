using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class SearchTests(WebAppFixture fixture)
{
    // M4.6: "search all of Austin" finds ads in every Austin category, and nothing from Boston.
    [Fact]
    public async Task SearchAllOfCity_ShowsRowsAcrossCategories_WithCategoryNames()
    {
        var keyword = "e2e" + Guid.NewGuid().ToString("N")[..8];
        await TestData.AddListingAsync(fixture.Services, "austin", "furniture", title: $"sofa {keyword}");
        await TestData.AddListingAsync(fixture.Services, "austin", "bikes", title: $"bike {keyword}");
        await TestData.AddListingAsync(fixture.Services, "boston", "bikes", title: $"boston bike {keyword}");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/austin");
            await page.GetByRole(AriaRole.Link, new() { Name = "search all of Austin" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/search\?city=austin$"));
            await page.GetByLabel("search", new() { Exact = true }).FillAsync(keyword);
            await page.GetByLabel("search", new() { Exact = true }).PressAsync("Enter");

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"q={keyword}"));
            var rows = page.Locator("li.listing-row");
            await Assertions.Expect(rows).ToHaveCountAsync(2);
            await Assertions.Expect(rows.Filter(new() { HasText = "sofa" }).Locator(".place")).ToContainTextAsync("furniture");
            await Assertions.Expect(rows.Filter(new() { HasText = "bike" }).Locator(".place")).ToContainTextAsync("bikes");
            await Assertions.Expect(page.GetByText("boston bike")).Not.ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }

    [Fact]
    public async Task TypeKeyword_PressEnter_UrlHasQueryAndNoPage()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/austin/furniture");
            await page.GetByLabel("search", new() { Exact = true }).FillAsync("chair");
            await page.GetByLabel("search", new() { Exact = true }).PressAsync("Enter");

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/austin/furniture\?q=chair"));
            Assert.DoesNotContain("page=", page.Url);
            await Assertions.Expect(page.GetByLabel("search", new() { Exact = true })).ToHaveValueAsync("chair");
        }
        finally { await page.CloseAsync(); }
    }
}
