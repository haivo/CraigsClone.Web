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
}
