using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class BrowseTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Home_ClickCity_ShowsThatCitysCategories()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/");
            await page.GetByRole(AriaRole.Link, new() { Name = "Austin", Exact = true }).ClickAsync();

            await page.WaitForURLAsync("**/austin");
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Austin");
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "furniture", Exact = true })).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }
}
