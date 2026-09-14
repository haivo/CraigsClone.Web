using Microsoft.AspNetCore.WebUtilities;

namespace CraigsClone.Web.ViewModels;

/// <summary>Input for the _Pager partial: where we are, how far it goes, and the filter to keep in every link.</summary>
public record PagerVm(int Page, int TotalPages, SearchFilterVm Filter)
{
    /// <summary>Page numbers to show: the current page plus up to 3 on each side, clamped to 1..TotalPages.</summary>
    public IEnumerable<int> Range()
    {
        var from = Math.Max(1, Page - 3);
        var to = Math.Min(TotalPages, Page + 3);
        for (var i = from; i <= to; i++) yield return i;
    }

    /// <summary>
    /// Link to a page, keeping the filter. Path-less ("?q=bike&page=2") so the browser resolves it
    /// against the current URL. "?" alone means "this path, no query", never "" (which would keep the old query).
    /// </summary>
    public string Href(int page)
    {
        var values = Filter.ToRouteValues(page);
        return values.Count == 0 ? "?" : QueryHelpers.AddQueryString("", values);
    }
}
