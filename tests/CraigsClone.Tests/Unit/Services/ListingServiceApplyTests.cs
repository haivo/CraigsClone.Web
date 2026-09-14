using CraigsClone.Web.Models;
using CraigsClone.Web.Services;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.Services;

// Tests the static Apply helper only. No database.
[Trait("Category", "Unit")]
public class ListingServiceApplyTests
{
    private static ListingFormVm Form() => new()
    {
        Title = "  Oak table  ",
        Description = "  Four legs.  ",
        Price = 50,
        CityId = 3,
        CategoryId = 7,
        Neighborhood = "  Hyde Park  ",
        ContactEmail = "  seller@example.com  ",
    };

    [Fact]
    public void CopiesEveryField_Trimmed()
    {
        var target = new Listing();

        ListingService.Apply(Form(), target);

        Assert.Equal("Oak table", target.Title);
        Assert.Equal("Four legs.", target.Description);
        Assert.Equal(50m, target.Price);
        Assert.Equal(3, target.CityId);
        Assert.Equal(7, target.CategoryId);
        Assert.Equal("Hyde Park", target.Neighborhood);
        Assert.Equal("seller@example.com", target.ContactEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankNeighborhood_BecomesNull(string? neighborhood)
    {
        var form = Form(); form.Neighborhood = neighborhood;
        var target = new Listing();

        ListingService.Apply(form, target);

        Assert.Null(target.Neighborhood);
    }

    [Fact]
    public void LeavesIdAndTimestampsAlone()
    {
        var created = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var target = new Listing { Id = 42, CreatedAt = created, UpdatedAt = created };

        ListingService.Apply(Form(), target);

        Assert.Equal(42, target.Id);
        Assert.Equal(created, target.CreatedAt);
        Assert.Equal(created, target.UpdatedAt);
    }
}
