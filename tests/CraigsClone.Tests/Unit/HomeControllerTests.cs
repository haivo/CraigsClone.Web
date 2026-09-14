using CraigsClone.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CraigsClone.Tests.Unit;

[Trait("Category", "Unit")]
public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsView()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }
}
