using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public static class CategoryGrouping
{
    /// <summary>Groups categories by their group, groups in enum order, categories in the order given.</summary>
    public static IReadOnlyList<IGrouping<CategoryGroup, Category>> Group(IEnumerable<Category> categories) =>
        categories.GroupBy(c => c.Group).OrderBy(g => g.Key).ToList();

    /// <summary>Heading text for a group, Craigslist-style lowercase.</summary>
    public static string Label(CategoryGroup group) => group switch
    {
        CategoryGroup.ForSale => "for sale",
        CategoryGroup.Housing => "housing",
        CategoryGroup.Jobs => "jobs",
        CategoryGroup.Services => "services",
        CategoryGroup.Community => "community",
        CategoryGroup.Gigs => "gigs",
        _ => group.ToString().ToLowerInvariant(),
    };
}

/// <summary>Input for the _CategoryGroups partial. CitySlug null = home page, names only, no links.</summary>
public record CategoryGroupsVm(IReadOnlyList<IGrouping<CategoryGroup, Category>> Groups, string? CitySlug);
