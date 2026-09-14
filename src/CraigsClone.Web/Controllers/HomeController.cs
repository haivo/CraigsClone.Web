using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CraigsClone.Web.Models;

namespace CraigsClone.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
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
