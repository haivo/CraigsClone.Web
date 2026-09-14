using System.Text.RegularExpressions;
using CraigsClone.Web.Data;
using CraigsClone.Web.Models;

namespace CraigsClone.Tests.Unit.Data;

// Checks the static seed lists only. No database.
[Trait("Category", "Unit")]
public class DbSeederDataTests
{
    private static IEnumerable<string> AllSlugs =>
        DbSeeder.Cities.Select(c => c.Slug).Concat(DbSeeder.Categories.Select(c => c.Slug));

    [Fact]
    public void HasSixCitiesAndTwentyTwoCategories()
    {
        Assert.Equal(6, DbSeeder.Cities.Count);
        Assert.Equal(22, DbSeeder.Categories.Count);
    }

    [Fact]
    public void CitySlugs_AreUnique()
    {
        var slugs = DbSeeder.Cities.Select(c => c.Slug).ToList();
        Assert.Equal(slugs.Count, slugs.Distinct().Count());
    }

    [Fact]
    public void CategorySlugs_AreUnique()
    {
        var slugs = DbSeeder.Categories.Select(c => c.Slug).ToList();
        Assert.Equal(slugs.Count, slugs.Distinct().Count());
    }

    // /{citySlug} would swallow these URLs otherwise.
    [Fact]
    public void NoSlug_IsAReservedRouteWord()
    {
        Assert.DoesNotContain(AllSlugs, DbSeeder.ReservedSlugs.Contains);
    }

    [Fact]
    public void AllSlugs_AreLowercaseUrlSafe()
    {
        var pattern = new Regex("^[a-z0-9-]+$");
        Assert.All(AllSlugs, slug => Assert.Matches(pattern, slug));
    }

    [Fact]
    public void EveryGroup_HasAtLeastOneCategory()
    {
        var groupsWithCategories = DbSeeder.Categories.Select(c => c.Group).ToHashSet();
        Assert.All(Enum.GetValues<CategoryGroup>(), g => Assert.Contains(g, groupsWithCategories));
    }
}
