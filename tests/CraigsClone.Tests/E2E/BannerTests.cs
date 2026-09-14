using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

// M5.3: the one-shot success banner after post, edit and delete.
[Trait("Category", "E2E")]
[Collection("app")]
public class BannerTests(WebAppFixture fixture)
{
    private static ILocator Banner(IPage page) => page.GetByRole(AriaRole.Status);

    [Fact]
    public async Task Post_ShowsBannerOnce_GoneAfterReload()
    {
        var title = TestData.UniqueTitle("Banner");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/post");
            await page.GetByLabel("title", new() { Exact = true }).FillAsync(title);
            await page.GetByLabel("description").FillAsync("d");
            await page.GetByLabel("city").SelectOptionAsync(new SelectOptionValue { Label = "Austin" });
            await page.GetByLabel("category").SelectOptionAsync(new SelectOptionValue { Label = "for sale: furniture" });
            await page.GetByLabel("contact email").FillAsync("me@example.com");
            await page.GetByRole(AriaRole.Button, new() { Name = "post" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"/listing/\d+$"));
            await Assertions.Expect(Banner(page)).ToHaveTextAsync("Your ad is posted.");

            await page.ReloadAsync();

            await Assertions.Expect(Banner(page)).Not.ToBeVisibleAsync();   // TempData is one-shot
        }
        finally { await page.CloseAsync(); }
    }

    [Fact]
    public async Task Edit_ShowsSavedBanner()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}/edit");
            await page.GetByLabel("title", new() { Exact = true }).FillAsync(listing.Title + " v2");
            await page.GetByRole(AriaRole.Button, new() { Name = "save" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"/listing/{listing.Id}$"));
            await Assertions.Expect(Banner(page)).ToHaveTextAsync("Saved.");
        }
        finally { await page.CloseAsync(); }
    }

    [Fact]
    public async Task Delete_ShowsDeletedBannerOnCategoryPage()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "austin", "furniture");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            page.Dialog += (_, dialog) => dialog.AcceptAsync();
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}");
            await page.GetByRole(AriaRole.Button, new() { Name = "delete" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("/austin/furniture$"));
            await Assertions.Expect(Banner(page)).ToHaveTextAsync("Ad deleted.");
        }
        finally { await page.CloseAsync(); }
    }
}
