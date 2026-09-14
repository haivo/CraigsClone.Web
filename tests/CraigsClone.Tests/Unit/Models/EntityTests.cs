using CraigsClone.Web.Models;

namespace CraigsClone.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class EntityTests
{
    // Guards against adding a group without updating the seed list (M1.4).
    [Fact]
    public void CategoryGroup_HasSixValues()
    {
        Assert.Equal(6, Enum.GetValues<CategoryGroup>().Length);
    }

    // Documents that "no price" is the default, not 0.
    [Fact]
    public void NewListing_HasNoPrice()
    {
        Assert.Null(new Listing().Price);
    }
}
