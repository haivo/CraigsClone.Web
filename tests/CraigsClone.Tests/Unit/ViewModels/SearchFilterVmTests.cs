using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class SearchFilterVmTests
{
    [Fact]
    public void Defaults_ProduceNoValues()
    {
        Assert.Empty(new SearchFilterVm().ToRouteValues());
    }

    [Fact]
    public void Keyword_IsIncludedTrimmed()
    {
        var values = new SearchFilterVm { Q = "  bike " }.ToRouteValues();

        Assert.Equal(new Dictionary<string, string?> { ["q"] = "bike" }, values);
    }

    [Fact]
    public void WhitespaceKeyword_IsIgnored()
    {
        Assert.Empty(new SearchFilterVm { Q = "   " }.ToRouteValues());
    }

    [Fact]
    public void NewestSort_IsOmitted_OthersAreLowercase()
    {
        Assert.DoesNotContain("sort", new SearchFilterVm { Sort = ListingSort.Newest }.ToRouteValues().Keys);
        Assert.Equal("priceasc", new SearchFilterVm { Sort = ListingSort.PriceAsc }.ToRouteValues()["sort"]);
        Assert.Equal("pricedesc", new SearchFilterVm { Sort = ListingSort.PriceDesc }.ToRouteValues()["sort"]);
    }

    [Fact]
    public void PageOne_IsOmitted_LaterPagesIncluded()
    {
        Assert.DoesNotContain("page", new SearchFilterVm { Page = 1 }.ToRouteValues().Keys);
        Assert.Equal("3", new SearchFilterVm { Page = 3 }.ToRouteValues()["page"]);
    }

    [Fact]
    public void PageArgument_OverridesPageProperty()
    {
        var filter = new SearchFilterVm { Page = 3 };

        Assert.Equal("2", filter.ToRouteValues(page: 2)["page"]);
        Assert.DoesNotContain("page", filter.ToRouteValues(page: 1).Keys);
    }

    [Fact]
    public void Prices_UseInvariantFormat()
    {
        var values = new SearchFilterVm { Min = 10.5m, Max = 200m }.ToRouteValues();

        Assert.Equal("10.5", values["min"]);
        Assert.Equal("200", values["max"]);
    }

    [Fact]
    public void CityAndCategory_AreIncludedWhenSet()
    {
        var values = new SearchFilterVm { City = "austin", Category = "bikes" }.ToRouteValues();

        Assert.Equal("austin", values["city"]);
        Assert.Equal("bikes", values["category"]);
    }
}
