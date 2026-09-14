using System.ComponentModel.DataAnnotations;
using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

// Runs the same DataAnnotations engine MVC uses, without a web server.
[Trait("Category", "Unit")]
public class ListingFormVmTests
{
    private static ListingFormVm Valid() => new()
    {
        Title = "Oak table",
        Description = "Four legs, one top.",
        Price = 50,
        CityId = 1,
        CategoryId = 1,
        Neighborhood = "Hyde Park",
        ContactEmail = "seller@example.com",
    };

    private static List<string> ErrorFields(ListingFormVm vm)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(vm, new ValidationContext(vm), results, validateAllProperties: true);
        return results.SelectMany(r => r.MemberNames).Distinct().ToList();
    }

    [Fact]
    public void ValidForm_HasNoErrors() => Assert.Empty(ErrorFields(Valid()));

    [Fact]
    public void EmptyTitle_IsError()
    {
        var vm = Valid(); vm.Title = "";
        Assert.Contains("Title", ErrorFields(vm));
    }

    [Theory]
    [InlineData(120, false)]
    [InlineData(121, true)]
    public void Title_MaxLengthIs120(int length, bool expectError)
    {
        var vm = Valid(); vm.Title = new string('x', length);
        Assert.Equal(expectError, ErrorFields(vm).Contains("Title"));
    }

    [Fact]
    public void BadEmail_IsError()
    {
        var vm = Valid(); vm.ContactEmail = "not-an-email";
        Assert.Contains("ContactEmail", ErrorFields(vm));
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(99_999_999.99, false)]
    [InlineData(100_000_000, true)]
    public void Price_MustBeZeroToColumnMax(double price, bool expectError)
    {
        var vm = Valid(); vm.Price = (decimal)price;
        Assert.Equal(expectError, ErrorFields(vm).Contains("Price"));
    }

    [Fact]
    public void NullPrice_IsAllowed()
    {
        var vm = Valid(); vm.Price = null;
        Assert.DoesNotContain("Price", ErrorFields(vm));
    }

    [Fact]
    public void MissingCity_IsError()
    {
        var vm = Valid(); vm.CityId = null;
        Assert.Contains("CityId", ErrorFields(vm));
    }

    [Fact]
    public void MissingCategory_IsError()
    {
        var vm = Valid(); vm.CategoryId = null;
        Assert.Contains("CategoryId", ErrorFields(vm));
    }

    [Theory]
    [InlineData(80, false)]
    [InlineData(81, true)]
    public void Neighborhood_MaxLengthIs80(int length, bool expectError)
    {
        var vm = Valid(); vm.Neighborhood = new string('x', length);
        Assert.Equal(expectError, ErrorFields(vm).Contains("Neighborhood"));
    }

    [Fact]
    public void NullNeighborhood_IsAllowed()
    {
        var vm = Valid(); vm.Neighborhood = null;
        Assert.DoesNotContain("Neighborhood", ErrorFields(vm));
    }
}
