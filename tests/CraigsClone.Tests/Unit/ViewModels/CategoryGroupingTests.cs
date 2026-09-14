using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class CategoryGroupingTests
{
    private static List<Category> SeedCategories() =>
        DbSeeder.Categories.Select(c => new Category { Group = c.Group, Name = c.Name, Slug = c.Slug }).ToList();

    [Fact]
    public void Group_ReturnsSixGroupsInEnumOrder()
    {
        var groups = CategoryGrouping.Group(SeedCategories());

        Assert.Equal(6, groups.Count);
        Assert.Equal(CategoryGroup.ForSale, groups[0].Key);
        Assert.Equal(CategoryGroup.Gigs, groups[^1].Key);
    }

    [Fact]
    public void Group_ForSaleContainsFurniture()
    {
        var groups = CategoryGrouping.Group(SeedCategories());

        var forSale = groups.Single(g => g.Key == CategoryGroup.ForSale);
        Assert.Contains(forSale, c => c.Slug == "furniture");
    }

    [Fact]
    public void Group_KeepsCategoryOrderWithinGroup()
    {
        var groups = CategoryGrouping.Group(SeedCategories());

        var forSale = groups.Single(g => g.Key == CategoryGroup.ForSale).Select(c => c.Slug).ToList();
        Assert.Equal(["furniture", "electronics", "bikes", "cars-trucks", "free"], forSale);
    }

    [Fact]
    public void Label_IsLowercaseWithSpaces()
    {
        Assert.Equal("for sale", CategoryGrouping.Label(CategoryGroup.ForSale));
        Assert.Equal("gigs", CategoryGrouping.Label(CategoryGroup.Gigs));
    }
}
