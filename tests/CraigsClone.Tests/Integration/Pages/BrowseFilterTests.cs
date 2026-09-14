using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

// M4.5: keyword, price and sort applied on the category page, over HTTP.
// Uses seattle/lost-found, which DevSeeder leaves empty, and a unique token in every title.
[Trait("Category", "Integration")]
[Collection("app")]
public class BrowseFilterTests(WebAppFixture fixture)
{
    private async Task<string> Seed()
    {
        var token = "tk" + Guid.NewGuid().ToString("N")[..8];
        await TestData.AddListingAsync(fixture.Services, "seattle", "lost-found", title: $"red chair {token}", price: 10);
        await TestData.AddListingAsync(fixture.Services, "seattle", "lost-found", title: $"blue chair {token}", price: 50);
        await TestData.AddListingAsync(fixture.Services, "seattle", "lost-found", title: $"table {token}", price: 30);
        return token;
    }

    [Fact]
    public async Task Keyword_NarrowsToMatches()
    {
        var token = await Seed();
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/seattle/lost-found?q=chair {token}");

        Assert.Contains($"red chair {token}", body);
        Assert.Contains($"blue chair {token}", body);
        Assert.DoesNotContain($"table {token}", body);
        Assert.Contains("2 results", body);
    }

    [Fact]
    public async Task MinPrice_DropsCheaperOnes()
    {
        var token = await Seed();
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/seattle/lost-found?q={token}&min=20");

        Assert.DoesNotContain($"red chair {token}", body);
        Assert.Contains($"blue chair {token}", body);
        Assert.Contains($"table {token}", body);
    }

    [Fact]
    public async Task MaxPrice_DropsDearerOnes()
    {
        var token = await Seed();
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/seattle/lost-found?q={token}&max=20");

        Assert.Contains($"red chair {token}", body);
        Assert.DoesNotContain($"blue chair {token}", body);
        Assert.DoesNotContain($"table {token}", body);
    }

    [Fact]
    public async Task SortPriceAsc_OrdersRows()
    {
        var token = await Seed();
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/seattle/lost-found?q={token}&sort=priceasc");

        var red = body.IndexOf($"red chair {token}", StringComparison.Ordinal);
        var table = body.IndexOf($"table {token}", StringComparison.Ordinal);
        var blue = body.IndexOf($"blue chair {token}", StringComparison.Ordinal);
        Assert.True(red < table && table < blue, $"order was red={red} table={table} blue={blue}");
    }

    [Fact]
    public async Task NoMatches_SaysNothingFound()
    {
        var token = await Seed();
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/seattle/lost-found?q={token} zzz");

        Assert.Contains("nothing found", body);
        Assert.DoesNotContain("no listings yet", body);
    }
}
