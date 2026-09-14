using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class PagerVmTests
{
    private static PagerVm Pager(int page, int total, SearchFilterVm? filter = null) => new(page, total, filter ?? new());

    [Theory]
    [InlineData(1, 10, new[] { 1, 2, 3, 4 })]
    [InlineData(5, 10, new[] { 2, 3, 4, 5, 6, 7, 8 })]
    [InlineData(10, 10, new[] { 7, 8, 9, 10 })]
    [InlineData(1, 1, new[] { 1 })]
    [InlineData(2, 3, new[] { 1, 2, 3 })]
    public void Range_IsCurrentPlusThreeEachSide_Clamped(int page, int total, int[] expected)
    {
        Assert.Equal(expected, Pager(page, total).Range());
    }

    [Fact]
    public void Href_KeepsFilter_AndSetsPage()
    {
        var pager = Pager(1, 5, new SearchFilterVm { Q = "bike" });

        Assert.Equal("?q=bike&page=2", pager.Href(2));
    }

    [Fact]
    public void Href_PageOne_OmitsPage()
    {
        var pager = Pager(2, 5, new SearchFilterVm { Q = "bike" });

        Assert.Equal("?q=bike", pager.Href(1));
    }

    [Fact]
    public void Href_EmptyFilter_IsJustPage()
    {
        Assert.Equal("?page=2", Pager(1, 5).Href(2));
    }

    // "?" not "": an empty href would resolve to the current URL, old query string included.
    [Fact]
    public void Href_EmptyFilter_PageOne_IsQuestionMark()
    {
        Assert.Equal("?", Pager(2, 5).Href(1));
    }

    [Fact]
    public void Href_EncodesKeyword()
    {
        var pager = Pager(1, 5, new SearchFilterVm { Q = "red & blue" });

        Assert.Equal("?q=red%20%26%20blue&page=2", pager.Href(2));
    }
}
