using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class PostListingTests(WebAppFixture fixture)
{
    [Fact]
    public async Task FillForm_Submit_LandsOnNewAd_AndItAppearsInCategory()
    {
        var title = TestData.UniqueTitle("Blue armchair");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/post");
            await page.GetByLabel("title", new() { Exact = true }).FillAsync(title);
            await page.GetByLabel("description").FillAsync("Comfortable, slightly faded.");
            await page.GetByLabel("price in $ (leave blank if not listed)").FillAsync("25");
            await page.GetByLabel("city").SelectOptionAsync(new SelectOptionValue { Label = "Austin" });
            await page.GetByLabel("category").SelectOptionAsync(new SelectOptionValue { Label = "for sale: furniture" });
            await page.GetByLabel("contact email").FillAsync("me@example.com");
            await page.GetByRole(AriaRole.Button, new() { Name = "post" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/listing/\d+$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(title);
            await Assertions.Expect(page.GetByText("$25")).ToBeVisibleAsync();

            await page.GotoAsync(fixture.BaseUrl + "/austin/furniture");
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = title, Exact = true })).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }

    // M3.7: clicking "post" while browsing Austin opens the form with Austin already chosen.
    [Fact]
    public async Task PostLinkFromCityPage_PreselectsThatCity()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/austin/furniture");
            await page.Locator("header").GetByRole(AriaRole.Link, new() { Name = "post", Exact = true }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/post\?city=austin$"));
            var selected = page.Locator("select[name=CityId] option:checked");
            await Assertions.Expect(selected).ToHaveTextAsync("Austin");
        }
        finally { await page.CloseAsync(); }
    }

    [Fact]
    public async Task SubmitWithoutTitle_ShowsError_AndKeepsWhatWasTyped()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/post");
            await page.GetByLabel("description").FillAsync("Keep me around.");
            await page.GetByRole(AriaRole.Button, new() { Name = "post" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("/post$"));
            await Assertions.Expect(page.GetByText("The Title field is required.")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByLabel("description")).ToHaveValueAsync("Keep me around.");
        }
        finally { await page.CloseAsync(); }
    }
}
