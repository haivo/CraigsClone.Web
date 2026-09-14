using System.Text.RegularExpressions;
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

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("/austin$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("Austin");
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "furniture", Exact = true })).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }

    // M2.5: the header links home, and shows the current city on city pages.
    [Fact]
    public async Task Header_BrandLink_GoesHome()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/austin/furniture");
            await Assertions.Expect(page.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Austin", Exact = true })).ToBeVisibleAsync();

            await page.Locator("header").GetByRole(AriaRole.Link, new() { Name = "CraigsClone", Exact = true }).ClickAsync();

            // Path must be exactly "/". A retrying assertion (not WaitForURL) so a failure shows the real URL.
            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"^https?://[^/]+/$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("CraigsClone");
        }
        finally { await page.CloseAsync(); }
    }

    // M2.4: category page lists the ad, clicking it opens the details page.
    [Fact]
    public async Task Category_ClickListing_ShowsDetails()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "austin", "furniture",
            description: "Green velvet, seats three.");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/austin/furniture");
            await page.GetByRole(AriaRole.Link, new() { Name = listing.Title, Exact = true }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"/listing/{listing.Id}$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(listing.Title);
            await Assertions.Expect(page.GetByText("Green velvet, seats three.")).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }
}
