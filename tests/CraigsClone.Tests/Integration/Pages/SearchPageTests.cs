using System.Net;
using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

[Trait("Category", "Integration")]
[Collection("app")]
public class SearchPageTests(WebAppFixture fixture)
{
    [Fact]
    public async Task Search_NoCity_SaysEverywhere()
    {
        using var client = fixture.CreateClient();

        var response = await client.GetAsync("/search");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<h1>search everywhere</h1>", body);
        Assert.DoesNotContain("type=\"hidden\" name=\"city\"", body);
    }

    [Fact]
    public async Task Search_WithCity_ScopesHeadingAndForm()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/search?city=austin");

        Assert.Contains("<h1>search in Austin</h1>", body);
        Assert.Contains("<input type=\"hidden\" name=\"city\" value=\"austin\"", body);
        Assert.Contains("class=\"city\" href=\"/austin\">Austin</a>", body);   // header shows the city too
    }

    [Theory]
    [InlineData("/search?city=nowhere")]
    [InlineData("/search?category=nothing")]
    public async Task Search_UnknownScope_Is404(string url)
    {
        using var client = fixture.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Search_RowsShowCityAndCategory()
    {
        var keyword = "pg" + Guid.NewGuid().ToString("N")[..8];
        await TestData.AddListingAsync(fixture.Services, "austin", "furniture", title: $"sofa {keyword}");
        await TestData.AddListingAsync(fixture.Services, "boston", "bikes", title: $"bike {keyword}");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/search?q={keyword}");

        Assert.Contains("2 results", body);
        Assert.Contains("Austin · <a href=\"/austin/furniture\">furniture</a>", body);
        Assert.Contains("Boston · <a href=\"/boston/bikes\">bikes</a>", body);
    }

    [Fact]
    public async Task Search_CityScope_ExcludesOtherCities()
    {
        var keyword = "cs" + Guid.NewGuid().ToString("N")[..8];
        await TestData.AddListingAsync(fixture.Services, "austin", "furniture", title: $"sofa {keyword}");
        await TestData.AddListingAsync(fixture.Services, "boston", "bikes", title: $"bike {keyword}");
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync($"/search?q={keyword}&city=austin");

        Assert.Contains($"sofa {keyword}", body);
        Assert.DoesNotContain($"bike {keyword}", body);
    }

    [Fact]
    public async Task CityPage_LinksToSearchAll()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/austin");

        Assert.Contains("href=\"/search?city=austin\"", body);
    }
}
