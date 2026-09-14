using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Services;

public class ListingService(AppDbContext db) : IListingService
{
    public async Task<PagedResult<Listing>> BrowseAsync(int cityId, int categoryId, int page, int pageSize = 20)
    {
        page = Math.Max(1, page);

        var query = db.Listings
            .Where(l => l.CityId == cityId && l.CategoryId == categoryId)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Listing>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
        };
    }

    public Task<Listing?> GetAsync(int id) =>
        db.Listings
            .Include(l => l.City)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id);
}
