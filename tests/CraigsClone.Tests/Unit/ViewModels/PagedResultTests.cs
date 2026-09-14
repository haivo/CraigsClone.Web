using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class PagedResultTests
{
    private static PagedResult<int> Make(int totalCount, int page, int pageSize = 20) => new()
    {
        Items = [],
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
    };

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(20, 1)]
    [InlineData(21, 2)]
    [InlineData(40, 2)]
    public void TotalPages_RoundsUp(int totalCount, int expectedPages)
    {
        Assert.Equal(expectedPages, Make(totalCount, page: 1).TotalPages);
    }

    [Theory]
    [InlineData(1, false, true)]    // first page of three
    [InlineData(2, true, true)]     // middle
    [InlineData(3, true, false)]    // last
    public void HasPreviousAndNext_ForFortyOneItems(int page, bool hasPrevious, bool hasNext)
    {
        var result = Make(totalCount: 41, page);

        Assert.Equal(hasPrevious, result.HasPrevious);
        Assert.Equal(hasNext, result.HasNext);
    }

    [Fact]
    public void EmptyResult_HasNoPrevOrNext()
    {
        var result = Make(totalCount: 0, page: 1);

        Assert.False(result.HasPrevious);
        Assert.False(result.HasNext);
    }
}
