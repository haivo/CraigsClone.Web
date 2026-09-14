using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public class HomeVm
{
    public required IReadOnlyList<City> Cities { get; init; }
    public required IReadOnlyList<IGrouping<CategoryGroup, Category>> Groups { get; init; }
}
