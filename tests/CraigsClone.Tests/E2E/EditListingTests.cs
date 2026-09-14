using System.Text.RegularExpressions;
using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class EditListingTests(WebAppFixture fixture)
{
    [Fact]
    public async Task ClickEdit_ChangeTitle_Save_ShowsNewTitle()
    {
        var listing = await TestData.AddListingAsync(fixture.Services);
        var newTitle = TestData.UniqueTitle("Renamed");
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}");
            await page.GetByRole(AriaRole.Link, new() { Name = "edit", Exact = true }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"/listing/{listing.Id}/edit$"));
            var titleBox = page.GetByLabel("title", new() { Exact = true });
            await Assertions.Expect(titleBox).ToHaveValueAsync(listing.Title);   // prefilled

            await titleBox.FillAsync(newTitle);
            await page.GetByRole(AriaRole.Button, new() { Name = "save" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex($"/listing/{listing.Id}$"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(newTitle);
        }
        finally { await page.CloseAsync(); }

        // And the database agrees, with UpdatedAt moved forward.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Listings.AsNoTracking().SingleAsync(l => l.Id == listing.Id);
        Assert.Equal(newTitle, stored.Title);
        Assert.True(stored.UpdatedAt > stored.CreatedAt);
    }
}
