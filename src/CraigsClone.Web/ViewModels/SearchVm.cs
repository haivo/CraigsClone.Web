using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public class SearchVm
{
    public required SearchFilterVm Filter { get; init; }
    public required PagedResult<Listing> Results { get; init; }
    public City? City { get; init; }
    public Category? Category { get; init; }

    public string Heading => (Category, City) switch
    {
        (not null, not null) => $"search {Category.Name} in {City.Name}",
        (null, not null) => $"search in {City.Name}",
        (not null, null) => $"search {Category.Name} everywhere",
        _ => "search everywhere",
    };
}
