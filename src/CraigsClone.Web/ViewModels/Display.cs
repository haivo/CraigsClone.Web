using System.Globalization;

namespace CraigsClone.Web.ViewModels;

/// <summary>Tiny formatting helpers shared by views. Invariant culture so output is the same everywhere.</summary>
public static class Display
{
    public static string Price(decimal? price) =>
        price is null ? "—" : "$" + price.Value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Posted(DateTime utc) =>
        utc.ToString("MMM d", CultureInfo.InvariantCulture);
}
