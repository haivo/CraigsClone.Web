namespace CraigsClone.Web.Models;

public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal? Price { get; set; }          // null means "price not listed"
    public int CityId { get; set; }
    public City City { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? Neighborhood { get; set; }
    public string ContactEmail { get; set; } = "";
    public DateTime CreatedAt { get; set; }      // always UTC
    public DateTime UpdatedAt { get; set; }      // always UTC
}
