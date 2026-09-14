using CraigsClone.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CraigsClone.Tests.Unit.Controllers;

// No dependencies, so a real unit test of a controller.
[Trait("Category", "Unit")]
public class ErrorControllerTests
{
    [Fact]
    public void Code404_ShowsNotFoundView()
    {
        var result = new ErrorController().Code(404);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("NotFound", view.ViewName);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(405)]
    [InlineData(400)]
    public void OtherCodes_ShowGenericView(int code)
    {
        var result = new ErrorController().Code(code);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    [Fact]
    public void Index_ReturnsView()
    {
        Assert.IsType<ViewResult>(new ErrorController().Index());
    }
}
