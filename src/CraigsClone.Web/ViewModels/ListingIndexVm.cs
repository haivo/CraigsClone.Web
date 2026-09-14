using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public class ListingIndexVm
{
    public required City City { get; init; }
    public required Category Category { get; init; }
    public required PagedResult<Listing> Results { get; init; }
    public required SearchFilterVm Filter { get; init; }

    /// <summary>True when no keyword, price or sort is active, so an empty page means "no listings yet" rather than "nothing found".</summary>
    public bool IsUnfiltered => Filter.ToRouteValues(page: 1).Count == 0;
}
