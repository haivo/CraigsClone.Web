using CraigsClone.Web.Models;

namespace CraigsClone.Web.ViewModels;

public class CityVm
{
    public required City City { get; init; }
    public required IReadOnlyList<IGrouping<CategoryGroup, Category>> Groups { get; init; }
}
