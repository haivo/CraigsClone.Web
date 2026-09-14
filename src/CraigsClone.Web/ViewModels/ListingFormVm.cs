using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CraigsClone.Web.ViewModels;

/// <summary>Used by both the post and edit forms. Never bind a form straight to the Listing entity.</summary>
public class ListingFormVm
{
    [Required, StringLength(120)]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    // numeric(10,2) tops out at 99,999,999.99; Postgres throws above that.
    [Range(0, 99_999_999.99)]
    public decimal? Price { get; set; }

    // int? not int: an empty dropdown posts nothing, which is null, which [Required] catches.
    // With plain int it would bind to 0 and slip through.
    [Required]
    public int? CityId { get; set; }

    [Required]
    public int? CategoryId { get; set; }

    [StringLength(80)]
    public string? Neighborhood { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string ContactEmail { get; set; } = "";

    // Filled by the controller for the dropdowns. Not posted by the form.
    public IEnumerable<SelectListItem> Cities { get; set; } = [];
    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
