using System.Globalization;

namespace CraigsClone.Web.ViewModels;

/// <summary>What the query string (?q=&min=&max=&sort=&page=) binds into on browse and search pages.</summary>
public class SearchFilterVm
{
    public string? Q { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public ListingSort Sort { get; set; } = ListingSort.Newest;
    public int Page { get; set; } = 1;

    // Only used by /search (M4.6). Null on a category page.
    public string? City { get; set; }
    public string? Category { get; set; }

    /// <summary>
    /// The filter as query-string values, for pager links. Defaults are left out so links stay short:
    /// ?q=bike&page=2 rather than ?q=bike&min=&max=&sort=newest&page=2.
    /// </summary>
    public Dictionary<string, string?> ToRouteValues(int? page = null)
    {
        var values = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(Q)) values["q"] = Q.Trim();
        if (Min is not null) values["min"] = Min.Value.ToString(CultureInfo.InvariantCulture);
        if (Max is not null) values["max"] = Max.Value.ToString(CultureInfo.InvariantCulture);
        if (Sort != ListingSort.Newest) values["sort"] = Sort.ToString().ToLowerInvariant();
        if (City is not null) values["city"] = City;
        if (Category is not null) values["category"] = Category;

        var p = page ?? Page;
        if (p > 1) values["page"] = p.ToString(CultureInfo.InvariantCulture);

        return values;
    }
}
