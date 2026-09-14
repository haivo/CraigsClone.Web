using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public class ListingIndexVm
{
    public required City City { get; init; }
    public required Category Category { get; init; }
    public required PagedResult<Listing> Results { get; init; }
    // M4.5 adds: public required SearchFilterVm Filter { get; init; }
}
