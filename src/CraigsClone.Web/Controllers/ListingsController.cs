using CraigsClone.Web.Data;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Controllers;

public class ListingsController(AppDbContext db, IListingService listings) : Controller
{
    // MVC fills `filter` from the query string (?q=&min=&max=&sort=&page=). A bad value
    // (sort=banana, page=abc) leaves that property at its default; we deliberately don't
    // check ModelState here, so a mangled URL degrades to "newest, page 1" instead of erroring.
    [HttpGet("{citySlug}/{categorySlug}")]
    public async Task<IActionResult> Index(string citySlug, string categorySlug, SearchFilterVm filter)
    {
        var city = await db.Cities.FirstOrDefaultAsync(c => c.Slug == citySlug);
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
        if (city is null || category is null) return NotFound();

        filter.City = null;       // fixed by the URL on this page; only /search uses these
        filter.Category = null;

        var results = await listings.BrowseAsync(city.Id, category.Id, filter);

        ViewData["Title"] = $"{category.Name} in {city.Name}";
        ViewData["CitySlug"] = city.Slug;
        ViewData["CityName"] = city.Name;
        return View(new ListingIndexVm { City = city, Category = category, Results = results, Filter = filter });
    }

    [HttpGet("listing/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var listing = await listings.GetAsync(id);
        if (listing is null) return NotFound();

        ViewData["Title"] = listing.Title;
        ViewData["CitySlug"] = listing.City.Slug;
        ViewData["CityName"] = listing.City.Name;
        return View(listing);
    }

    [HttpGet("post")]
    public async Task<IActionResult> Create(string? city)
    {
        var form = new ListingFormVm();
        if (city is not null)
        {
            // Preselect the city the user was browsing. Unknown slug: just leave it blank.
            form.CityId = await db.Cities.Where(c => c.Slug == city).Select(c => (int?)c.Id).FirstOrDefaultAsync();
        }
        await FillSelectListsAsync(form);

        ViewData["Title"] = "post an ad";
        return View(form);
    }

    [HttpPost("post")]
    public async Task<IActionResult> Create(ListingFormVm form)
    {
        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(form);   // dropdowns aren't posted back, so refill them
            ViewData["Title"] = "post an ad";
            return View(form);
        }

        var listing = await listings.CreateAsync(form);
        TempData["Success"] = "Your ad is posted.";
        return RedirectToAction(nameof(Details), new { id = listing.Id });
    }

    [HttpGet("listing/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var listing = await listings.GetAsync(id);
        if (listing is null) return NotFound();

        var form = ListingFormVm.From(listing);
        await FillSelectListsAsync(form);

        ViewData["Title"] = "edit ad";
        ViewData["CitySlug"] = listing.City.Slug;
        ViewData["CityName"] = listing.City.Name;
        return View(form);
    }

    [HttpPost("listing/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, ListingFormVm form)
    {
        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(form);
            ViewData["Title"] = "edit ad";
            return View(form);
        }

        if (!await listings.UpdateAsync(id, form)) return NotFound();

        TempData["Success"] = "Saved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST, not GET: a crawler or a browser prefetch must never be able to delete an ad.
    [HttpPost("listing/{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await listings.GetAsync(id);   // need its city and category for the redirect
        if (listing is null) return NotFound();

        await listings.DeleteAsync(id);

        TempData["Success"] = "Ad deleted.";
        return Redirect($"/{listing.City.Slug}/{listing.Category.Slug}");
    }

    private async Task FillSelectListsAsync(ListingFormVm form)
    {
        form.Cities = await db.Cities
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        var categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();
        form.Categories = categories
            .Select(c => new SelectListItem($"{CategoryGrouping.Label(c.Group)}: {c.Name}", c.Id.ToString()))
            .ToList();
    }
}
