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

            await page.WaitForURLAsync($"**/listing/{listing.Id}");
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(listing.Title);
            await Assertions.Expect(page.GetByText("Green velvet, seats three.")).ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }
    }
}
