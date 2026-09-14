using Microsoft.Playwright;

namespace CraigsClone.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("app")]
public class HomePageTests(WebAppFixture fixture)
{
    [Fact]
    public async Task HomePage_Loads()
    {
        var page = await fixture.Browser.NewPageAsync();
        try
        {
            var resp = await page.GotoAsync(fixture.BaseUrl + "/");

            Assert.NotNull(resp);
            Assert.True(resp.Ok, $"status {resp.Status}");
            Assert.Contains("CraigsClone", await page.TitleAsync());
        }
        finally { await page.CloseAsync(); }
    }

    // M0.7: after stripping Bootstrap/jQuery, no page may still reference a deleted file.
    [Fact]
    public async Task HomePage_HasNoFailedRequests()
    {
        var page = await fixture.Browser.NewPageAsync();
        var problems = new List<string>();
        page.RequestFailed += (_, r) => problems.Add($"failed: {r.Url}");
        page.Response += (_, r) => { if (r.Status >= 400) problems.Add($"{r.Status}: {r.Url}"); };
        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/", new() { WaitUntil = WaitUntilState.NetworkIdle });

            Assert.Empty(problems);
        }
        finally { await page.CloseAsync(); }
    }
}
