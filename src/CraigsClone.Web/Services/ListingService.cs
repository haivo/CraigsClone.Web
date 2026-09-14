using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Services;

public class ListingService(AppDbContext db) : IListingService
{
    public const int MaxPageSize = 100;

    public Task<PagedResult<Listing>> BrowseAsync(int cityId, int categoryId, SearchFilterVm filter, int pageSize = 20)
    {
        var scoped = db.Listings.Where(l => l.CityId == cityId && l.CategoryId == categoryId);
        return ToPagedAsync(Query(scoped, filter), filter.Page, pageSize);
    }

    public Task<Listing?> GetAsync(int id) =>
        db.Listings
            .Include(l => l.City)
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id);

    // The service is the ONLY place timestamps are set, and always UtcNow: Npgsql rejects local times.

    public async Task<Listing> CreateAsync(ListingFormVm form)
    {
        var listing = new Listing();
        Apply(form, listing);
        listing.CreatedAt = listing.UpdatedAt = DateTime.UtcNow;

        db.Listings.Add(listing);
        await db.SaveChangesAsync();
        return listing;
    }

    public async Task<bool> UpdateAsync(int id, ListingFormVm form)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return false;

        Apply(form, listing);
        listing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return false;

        db.Listings.Remove(listing);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Copies form fields onto an entity. Shared by create and update so they can't drift apart.</summary>
    public static void Apply(ListingFormVm form, Listing target)
    {
        target.Title = form.Title.Trim();
        target.Description = form.Description.Trim();
        target.Price = form.Price;
        target.CityId = form.CityId!.Value;          // validated [Required] before we get here
        target.CategoryId = form.CategoryId!.Value;
        target.Neighborhood = string.IsNullOrWhiteSpace(form.Neighborhood) ? null : form.Neighborhood.Trim();
        target.ContactEmail = form.ContactEmail.Trim();
    }

    // ---- Query building. Static where possible so the pure parts can be unit-tested without Postgres. ----

    /// <summary>Applies keyword, price range and sort from the filter. Paging is separate (ToPagedAsync).</summary>
    public IQueryable<Listing> Query(IQueryable<Listing> source, SearchFilterVm filter)
    {
        var q = source;

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            // ILIKE = case-insensitive LIKE. Postgres only, which is why these tests need a real database.
            var pattern = $"%{EscapeLike(filter.Q.Trim())}%";
            q = q.Where(l => EF.Functions.ILike(l.Title, pattern, "\\")
                          || EF.Functions.ILike(l.Description, pattern, "\\"));
        }

        // A null price fails both comparisons, so unpriced ads drop out whenever a bound is set. Intended.
        if (filter.Min is not null) q = q.Where(l => l.Price >= filter.Min);
        if (filter.Max is not null) q = q.Where(l => l.Price <= filter.Max);

        return ApplySort(q, filter.Sort);
    }

    /// <summary>Escapes LIKE wildcards so "50%" searches for a literal percent sign.</summary>
    public static string EscapeLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    /// Sort order. Price sorts put unpriced ads last in BOTH directions: Postgres would otherwise
    /// put NULLs first on a descending sort, so "high to low" would lead with "price not listed".
    /// </summary>
    public static IOrderedQueryable<Listing> ApplySort(IQueryable<Listing> q, ListingSort sort) => sort switch
    {
        ListingSort.PriceAsc => q.OrderBy(l => l.Price == null).ThenBy(l => l.Price).ThenByDescending(l => l.CreatedAt),
        ListingSort.PriceDesc => q.OrderBy(l => l.Price == null).ThenByDescending(l => l.Price).ThenByDescending(l => l.CreatedAt),
        _ => q.OrderByDescending(l => l.CreatedAt),
    };

    /// <summary>Runs the count and the page query. Page below 1 becomes 1; page size is capped.</summary>
    public static async Task<PagedResult<T>> ToPagedAsync<T>(IQueryable<T> q, int page, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}
