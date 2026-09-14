using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class DeleteListingTests(WebAppFixture fixture)
{
    // Playwright dismisses dialogs by default, so each test decides what to do with the confirm.

    [Fact]
    public async Task ClickDelete_Confirm_RemovesAd_AndReturnsToCategory()
    {
        var listing = await TestData.AddListingAsync(fixture.Services, "austin", "furniture");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            page.Dialog += (_, dialog) => dialog.AcceptAsync();
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}");
            await page.GetByRole(AriaRole.Button, new() { Name = "delete" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex("/austin/furniture$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = listing.Title, Exact = true })).Not.ToBeVisibleAsync();
        }
        finally { await page.CloseAsync(); }

        using var client = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/listing/{listing.Id}")).StatusCode);
    }

    [Fact]
    public async Task ClickDelete_Cancel_KeepsAd()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            page.Dialog += (_, dialog) => dialog.DismissAsync();
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}");
            await page.GetByRole(AriaRole.Button, new() { Name = "delete" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"/listing/{listing.Id}$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(listing.Title);
        }
        finally { await page.CloseAsync(); }

        using var client = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/listing/{listing.Id}")).StatusCode);
    }
}
