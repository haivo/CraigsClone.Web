using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Web.Services;

/// <summary>All listing queries live behind this so controllers stay thin and tests hit it directly.</summary>
public interface IListingService
{
    /// <summary>Newest-first listings for one city and category. Pages below 1 are treated as 1.</summary>
    Task<PagedResult<Listing>> BrowseAsync(int cityId, int categoryId, int page, int pageSize = 20);

    /// <summary>One listing with its City and Category loaded, or null.</summary>
    Task<Listing?> GetAsync(int id);
}
