using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

// M5.1: at phone width, no page may scroll sideways.
[Trait("Category", "E2E")]
[Collection("app")]
public class ResponsiveTests(WebAppFixture fixture)
{
    private const string OverflowCheck =
        "() => document.documentElement.scrollWidth > document.documentElement.clientWidth";

    [Theory]
    [InlineData("/")]
    [InlineData("/austin")]
    [InlineData("/austin/furniture")]
    [InlineData("/post")]
    [InlineData("/search?city=austin")]
    public async Task Page_HasNoHorizontalOverflow_At400px(string path)
    {
        var page = await fixture.Browser.NewPageAsync(new() { ViewportSize = new() { Width = 400, Height = 800 } });
        try
        {
            await page.GotoAsync(fixture.BaseUrl + path);

            Assert.False(await page.EvaluateAsync<bool>(OverflowCheck), $"{path} scrolls horizontally at 400px");
        }
        finally { await page.CloseAsync(); }
    }

    [Fact]
    public async Task DetailsPage_HasNoHorizontalOverflow_At400px()
    {
        var listing = await TestData.AddListingAsync(fixture.Services,
            title: "A fairly long title that needs to wrap on a narrow phone screen without overflowing",
            description: "Line one.\nLine two is quite a bit longer and should wrap rather than push the page wider.",
            neighborhood: "South Congress");
        var page = await fixture.Browser.NewPageAsync(new() { ViewportSize = new() { Width = 400, Height = 800 } });
        try
        {
            await page.GotoAsync($"{fixture.BaseUrl}/listing/{listing.Id}");

            Assert.False(await page.EvaluateAsync<bool>(OverflowCheck), "details page scrolls horizontally at 400px");
        }
        finally { await page.CloseAsync(); }
    }
}
