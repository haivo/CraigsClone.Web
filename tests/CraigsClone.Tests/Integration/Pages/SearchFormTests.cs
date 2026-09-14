using CraigsClone.Tests.E2E;

namespace CraigsClone.Tests.Integration.Pages;

// M4.4: the search form on the category page. Renders from M4.5.
[Trait("Category", "Integration")]
[Collection("app")]
public class SearchFormTests(WebAppFixture fixture)
{
    [Fact(Skip = "Form renders in M4.5")]
    public async Task CategoryPage_HasGetSearchForm()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/austin/furniture");

        Assert.Contains("<form method=\"get\" class=\"search\"", body);
        Assert.Contains("name=\"q\"", body);
        Assert.Contains("name=\"sort\"", body);
    }

    // The form remembers what you searched for.
    [Fact(Skip = "Form renders in M4.5")]
    public async Task CategoryPage_FormReflectsQueryString()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/austin/furniture?q=chair&min=5&max=50&sort=pricedesc");

        Assert.Contains("name=\"q\" value=\"chair\"", body);
        Assert.Contains("name=\"min\" type=\"number\" step=\"0.01\" min=\"0\" value=\"5\"", body);
        Assert.Contains("name=\"max\" type=\"number\" step=\"0.01\" min=\"0\" value=\"50\"", body);
        Assert.Contains("<option value=\"pricedesc\" selected=\"selected\">", body);
        Assert.DoesNotContain("<option value=\"newest\" selected", body);
    }

    // city/category hidden inputs are only for /search (M4.6); a category page's URL already fixes both.
    [Fact(Skip = "Form renders in M4.5")]
    public async Task CategoryPage_FormHasNoHiddenCityOrCategory()
    {
        using var client = fixture.CreateClient();

        var body = await client.GetStringAsync("/austin/furniture");

        Assert.DoesNotContain("type=\"hidden\" name=\"city\"", body);
        Assert.DoesNotContain("type=\"hidden\" name=\"category\"", body);
    }
}
