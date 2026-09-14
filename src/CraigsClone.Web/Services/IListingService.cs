using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Web.Services;

/// <summary>All listing queries and changes live behind this so controllers stay thin and tests hit it directly.</summary>
public interface IListingService
{
    /// <summary>Listings for one city and category, filtered, sorted and paged per the filter.</summary>
    Task<PagedResult<Listing>> BrowseAsync(int cityId, int categoryId, SearchFilterVm filter, int pageSize = 20);

    /// <summary>
    /// Listings across categories (and cities), filtered, sorted and paged. Null cityId or categoryId
    /// means "any". Rows come back with City and Category loaded, since the page shows both.
    /// </summary>
    Task<PagedResult<Listing>> SearchAsync(SearchFilterVm filter, int? cityId, int? categoryId, int pageSize = 20);

    /// <summary>One listing with its City and Category loaded, or null.</summary>
    Task<Listing?> GetAsync(int id);

    /// <summary>Creates a listing from the form. Sets both timestamps to now (UTC).</summary>
    Task<Listing> CreateAsync(ListingFormVm form);

    /// <summary>Applies the form to an existing listing and bumps UpdatedAt. False if not found.</summary>
    Task<bool> UpdateAsync(int id, ListingFormVm form);

    /// <summary>Deletes a listing. False if not found.</summary>
    Task<bool> DeleteAsync(int id);
}
