using CraigsClone.Web.Data;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Controllers;

public class ListingsController(AppDbContext db, IListingService listings) : Controller
{
    [HttpGet("{citySlug}/{categorySlug}")]
    public async Task<IActionResult> Index(string citySlug, string categorySlug, int page = 1)
    {
        var city = await db.Cities.FirstOrDefaultAsync(c => c.Slug == citySlug);
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
        if (city is null || category is null) return NotFound();

        var results = await listings.BrowseAsync(city.Id, category.Id, page);

        ViewData["Title"] = $"{category.Name} in {city.Name}";
        ViewData["CitySlug"] = city.Slug;
        ViewData["CityName"] = city.Name;
        return View(new ListingIndexVm { City = city, Category = category, Results = results });
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
}
