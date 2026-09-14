using Microsoft.AspNetCore.Mvc;

namespace CraigsClone.Web.Controllers;

// [Route], not [HttpGet]: UseStatusCodePagesWithReExecute replays the ORIGINAL request against
// these routes, and a rejected POST (for example a missing anti-forgery token) is still a POST.
// A GET-only route would answer 405 and overwrite the real status code.
// [IgnoreAntiforgeryToken]: that replayed POST has no token either; the global filter must not
// reject it a second time. These actions only render a view, so there is nothing to protect.
[IgnoreAntiforgeryToken]
public class ErrorController : Controller
{
    // Unhandled exception (outside Development, where the developer page shows instead).
    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        ViewData["Title"] = "something went wrong";
        return View();
    }

    // Any non-success status with an empty body (404 from NotFound(), 400, 405, ...) is re-executed here.
    // The original status code is kept, because a ViewResult never sets one; only the body changes.
    [Route("error/{code:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Code(int code)
    {
        if (code == 404)
        {
            ViewData["Title"] = "page not found";
            return View("NotFound");
        }

        ViewData["Title"] = "something went wrong";
        return View("Index");
    }
}
