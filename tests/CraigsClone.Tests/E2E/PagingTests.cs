using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class PagingTests(WebAppFixture fixture)
{
    [Fact]
    public async Task ClickNext_ShowsSecondPage()
    {
        await TestData.AddListingsAsync(fixture.Services, "chicago", "lost-found", 25, "paging");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/chicago/lost-found");
            await Assertions.Expect(page.Locator("li.listing-row")).ToHaveCountAsync(20);

            await page.GetByRole(AriaRole.Link, new() { Name = "next", Exact = true }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"\?page=2$"));
            await Assertions.Expect(page.Locator("li.listing-row")).ToHaveCountAsync(5);
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "next", Exact = true })).Not.ToBeVisibleAsync();
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "prev", Exact = true })).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }
}
