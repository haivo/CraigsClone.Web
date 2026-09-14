using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class SearchTests(WebAppFixture fixture)
{
    [Fact(Skip = "Form renders in M4.5")]
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
