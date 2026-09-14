using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class ErrorPagesTests(WebAppFixture fixture)
{
    [Fact]
    public async Task BadUrl_ShowsNotFound_AndHomeLinkWorks()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            var response = await page.GotoAsync(fixture.BaseUrl + "/nowhere/nothing");

            Assert.Equal(404, response!.Status);
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync("page not found");
            Assert.EndsWith("/nowhere/nothing", page.Url);   // address bar keeps the URL the user typed

            await page.GetByRole(AriaRole.Link, new() { Name = "go back to the home page" }).ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@"^https?://[^/]+/$"));
        }
        finally { await page.CloseAsync(); }
    }
}
