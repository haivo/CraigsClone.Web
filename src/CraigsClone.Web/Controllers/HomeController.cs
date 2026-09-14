using System.Diagnostics;
using CraigsClone.Web.Data;
using CraigsClone.Web.Models;
using CraigsClone.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CraigsClone.Web.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var cities = await db.Cities.OrderBy(c => c.Name).ToListAsync();
        var categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();   // seed order

        ViewData["Title"] = "home";
        return View(new HomeVm { Cities = cities, Groups = CategoryGrouping.Group(categories) });
    }

    [HttpGet("{citySlug}")]
    public async Task<IActionResult> City(string citySlug)
    {
        var city = await db.Cities.FirstOrDefaultAsync(c => c.Slug == citySlug);
        if (city is null) return NotFound();

        var categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();

        ViewData["Title"] = city.Name;
        ViewData["CitySlug"] = city.Slug;
        ViewData["CityName"] = city.Name;
        return View(new CityVm { City = city, Groups = CategoryGrouping.Group(categories) });
    }

    // Kept from the template so UseExceptionHandler("/Home/Error") still has a target.
    // M5.2 replaces this with a dedicated ErrorController.
    [HttpGet("Home/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
