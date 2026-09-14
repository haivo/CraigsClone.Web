using CraigsClone.Web.ViewModels;

namespace CraigsClone.Tests.Unit.ViewModels;

[Trait("Category", "Unit")]
public class DisplayTests
{
    [Fact]
    public void Price_Null_IsDash() => Assert.Equal("—", Display.Price(null));

    [Fact]
    public void Price_Thousands_HasSeparator() => Assert.Equal("$1,500", Display.Price(1500m));

    // Free items show a price, not a dash.
    [Fact]
    public void Price_Zero_IsZeroDollars() => Assert.Equal("$0", Display.Price(0m));

    [Fact]
    public void Posted_IsMonthAndDay() =>
        Assert.Equal("Mar 7", Display.Posted(new DateTime(2026, 3, 7, 12, 0, 0, DateTimeKind.Utc)));
}
