using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class ListingFormVmFromTests
{
    private static Listing Entity() => new()
    {
        Id = 9,
        Title = "Oak table",
        Description = "Four legs.",
        Price = 50,
        CityId = 3,
        CategoryId = 7,
        Neighborhood = "Hyde Park",
        ContactEmail = "seller@example.com",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public void From_CopiesEveryEditableField()
    {
        var form = ListingFormVm.From(Entity());

        Assert.Equal("Oak table", form.Title);
        Assert.Equal("Four legs.", form.Description);
        Assert.Equal(50m, form.Price);
        Assert.Equal(3, form.CityId);
        Assert.Equal(7, form.CategoryId);
        Assert.Equal("Hyde Park", form.Neighborhood);
        Assert.Equal("seller@example.com", form.ContactEmail);
    }

    // From then Apply must give back the same fields: the two mappers are inverses.
    [Fact]
    public void From_ThenApply_RoundTrips()
    {
        var original = Entity();
        var copy = new Listing();

        ListingService.Apply(ListingFormVm.From(original), copy);

        Assert.Equal(original.Title, copy.Title);
        Assert.Equal(original.Description, copy.Description);
        Assert.Equal(original.Price, copy.Price);
        Assert.Equal(original.CityId, copy.CityId);
        Assert.Equal(original.CategoryId, copy.CategoryId);
        Assert.Equal(original.Neighborhood, copy.Neighborhood);
        Assert.Equal(original.ContactEmail, copy.ContactEmail);
    }
}
